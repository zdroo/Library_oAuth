using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Library_oAuth.Controllers;
using Library_oAuth.Models;
using Library_oAuth.Repositories;
using Library_oAuth.Repositories.Interfaces;
using Library_oAuth.Services;
using Xunit.Sdk;

namespace Library_oAuth.Test.ControllerTests
{
    public class LibraryControllerTests
    {
        private readonly IDataRepository _dataRepository;
        private readonly IDateTimeProvider _dateTimeProvider;
        public LibraryControllerTests()
        {
            _dataRepository = A.Fake<IDataRepository>();
            _dateTimeProvider = A.Fake<IDateTimeProvider>();
        }

        [Fact]
        public void LibraryController_GetBooks_RetursnOK()
        {
            var controller = new LibraryController(_dataRepository);

            //Act
            var result = controller.GetBooks().Result;

            //Assert
            result.Should().BeOfType(typeof(OkObjectResult));
        }
        [Fact]
        public void GetBookByIsbn_ValidIsbn_ReturnsOk()
        {
            // Arrange
            var isbn = "1234567890";
            var expectedBook = new BookModel { Id = isbn, Name = "Test Book" };

            A.CallTo(() => _dataRepository.GetBookByIsbn(isbn)).Returns(expectedBook);

            var controller = new LibraryController(_dataRepository);

            // Act
            var result = controller.GetBooksByIsbn(isbn).Result;

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                .Which.StatusCode.Should().Be(200);

            result.As<OkObjectResult>().Value.Should().BeEquivalentTo(expectedBook);
        }
        [Fact]
        public void GetBookByIsbn_NotFoundIsbn_ReturnsBadRequest()
        {
            // Arrange
            var isbn = "1234567890";
            string expectedErrorMessage = $"Book with isbn {isbn} not found.";
            var fakeBookLoan = A.Fake<BookLoanModel>();

            A.CallTo(() => _dataRepository.GetBookByIsbn(isbn)).Returns(Task.FromResult<BookModel>(null));

            var controller = new LibraryController(_dataRepository);

            // Act
            var result = controller.GetBooksByIsbn(isbn).Result;

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.StatusCode.Should().Be(400);

            result.As<BadRequestObjectResult>().Value.Should().Be(expectedErrorMessage);
        }
        [Fact]
        public void BorrowBook_WithValidIsbn_ReturnsOk()
        {
            // Arrange
            int userId = 1;
            var fakeInsertedLoan = new BookLoanModel{ IsActive = true };

            var fakeBookLoan = A.Fake<BookLoanModel>();

            string isbn = "1234567890";
            var claims = new List<Claim> { new Claim("Id",userId.ToString()) };
            var userClaimsIdentity = new ClaimsIdentity(claims);
            var userClaimsPrincipal = new ClaimsPrincipal(userClaimsIdentity);

            var controller = new LibraryController(_dataRepository);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = userClaimsPrincipal
                }
            };

            DataService dataService = new DataService(_dataRepository);

            // Set up the required dependencies for the test
            A.CallTo(() => _dataRepository.GetUserActiveLoan(A<int>._)).Returns(Task.FromResult<BookLoanModel>(null));
            A.CallTo(() => _dataRepository.GetBookByIsbn(isbn)).Returns(Task.FromResult<BookModel>(new BookModel { Id = isbn, Pieces = 10 }));
            A.CallTo(() => _dataRepository.BorrowBook(A<BookLoanModel>._)).Returns(Task.FromResult<BookLoanModel>(fakeInsertedLoan));

            // Act
            var result = controller.BorrowBook(isbn).Result;

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeSameAs(fakeInsertedLoan);
        }
        [Fact]
        public void BorrowBook_WithActiveLoan_ReturnsBadRequest()
        {
            // Arrange
            var controller = new LibraryController(_dataRepository);
            var fakeInsertedLoan = new BookLoanModel { IsActive = true };
            var userId = 1; // Replace with desired user ID
            var isbn = "12345"; // Replace with desired ISBN

            var claims = new List<Claim> { new Claim("Id", userId.ToString()) };
            var userClaimsIdentity = new ClaimsIdentity(claims);
            var userClaimsPrincipal = new ClaimsPrincipal(userClaimsIdentity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = userClaimsPrincipal
                }
            };

            var activeLoan = fakeInsertedLoan; // Replace with desired active loan object
            A.CallTo(() => _dataRepository.GetBookByIsbn(isbn)).Returns(Task.FromResult<BookModel>(new BookModel()));
            A.CallTo(() => _dataRepository.GetUserActiveLoan(userId)).Returns(Task.FromResult<BookLoanModel>(activeLoan));

            // Act
            var result = controller.BorrowBook(isbn).Result;

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("You already have an active loan. Come back after you return it.");
        }
        [Fact]
        public void BorrowBook_NotFoundBook_Returns500()
        {
            // Arrange
            var controller = new LibraryController(_dataRepository);
            var fakeInsertedLoan = new BookLoanModel { IsActive = true };
            var userId = 1; // Replace with desired user ID
            var isbn = "12345"; // Replace with desired ISBN

            var claims = new List<Claim> { new Claim("Id", userId.ToString()) };
            var userClaimsIdentity = new ClaimsIdentity(claims);
            var userClaimsPrincipal = new ClaimsPrincipal(userClaimsIdentity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = userClaimsPrincipal
                }
            };

            //var activeLoan = fakeInsertedLoan; // Replace with desired active loan object
            A.CallTo(() => _dataRepository.GetBookByIsbn(isbn)).Returns(Task.FromResult<BookModel>(null));
            A.CallTo(() => _dataRepository.GetUserActiveLoan(userId)).Returns(Task.FromResult<BookLoanModel>(null));

            // Act
            var result = controller.BorrowBook(isbn).Result;

            // Assert
            result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);

            var statusCodeResult = result as ObjectResult;
            statusCodeResult.Value.Should().Be("Book not found or unavailable.");
        }
        [Fact]
        public void ReturnBook_WithActiveLoan_ReturnsOk()
        {
            // Arrange
            var controller = new LibraryController(_dataRepository);
            var userId = 1;
            var fakeInactiveLoan = new BookLoanModel { IsActive = false, ReturnDate = new DateTime(2023, 7, 10), Price = 10 };
            var fakeActiveLoan = new BookLoanModel { IsActive = true};
            decimal delayCost = Convert.ToInt32((DateTime.Now - new DateTime(2023, 7, 10)).TotalDays) * 0.1m;

            var claims = new List<Claim> { new Claim("Id", userId.ToString()) };
            var userClaimsIdentity = new ClaimsIdentity(claims);
            var userClaimsPrincipal = new ClaimsPrincipal(userClaimsIdentity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = userClaimsPrincipal
                }
            };

            A.CallTo(() => _dataRepository.GetUserActiveLoan(userId)).Returns(Task.FromResult<BookLoanModel>(fakeActiveLoan));
            A.CallTo(() => _dataRepository.ReturnBook(A<BookLoanModel>._)).Returns(Task.FromResult<BookLoanModel>(fakeInactiveLoan));
            // Act
            var result = controller.ReturnBook().Result;

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeSameAs(fakeInactiveLoan);

            var resObj = okResult.Value as BookLoanModel;
            resObj.DelayCost.Should().BeApproximately(delayCost, precision: 0.0001m);
        }
        [Fact]
        public void ReturnBook_WithNoActiveLoans_ReturnsBadRequest()
        {
            // Arrange
            var controller = new LibraryController(_dataRepository);
            var userId = 1; 

            var claims = new List<Claim> { new Claim("Id", userId.ToString()) };
            var userClaimsIdentity = new ClaimsIdentity(claims);
            var userClaimsPrincipal = new ClaimsPrincipal(userClaimsIdentity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = userClaimsPrincipal
                }
            };

            A.CallTo(() => _dataRepository.GetUserActiveLoan(userId)).Returns(Task.FromResult<BookLoanModel>(null));

            // Act
            var result = controller.ReturnBook().Result;

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("You have no active loans.");
        }
        [Fact]
        public void AddBook_ReturnsOk()
        {
            // Arrange
            var controller = new LibraryController(_dataRepository);
            var bookDto = new BookDto { Price = 50, Author = "Uriens", Id = "111111", Name = "Zoro", Pieces = 20 };
            var insertedBook = new BookModel { Price = 50, Author = "Uriens", Id = "111111", Name = "Zoro", Pieces = 20, ItemType = ItemTypes.Book };

            A.CallTo(() => _dataRepository.InsertBook(A<BookDto>._)).Returns(Task.FromResult<BookModel>(insertedBook));
            // Act
            var result = controller.AddBook(bookDto).Result;

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().BeSameAs(insertedBook);
        }
        //[Theory]
        //[InlineData("2023-07-10", "2023-07-18", 0.5)]
        //public void TestDelay(DateTime returnDate, DateTime today, decimal delayCost)
        //{

        //    A.CallTo(() => _dateTimeProvider.GetToday()).Returns(today);


        //}
    }
}
