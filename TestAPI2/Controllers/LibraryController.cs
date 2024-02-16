using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics;
using Library_oAuth.Models;
using Library_oAuth.Repositories.Interfaces;
using Library_oAuth.Services;

namespace Library_oAuth.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LibraryController : ControllerBase
    {
        private readonly IDataRepository _dataRepository;
        public LibraryController(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }
        [HttpGet]
        [ActionName("GetBooks")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBooks()
        {
            try
            {
                DataService dataService = new DataService(_dataRepository);
                List<BookDto> books = await dataService.GetBooks();
                return Ok(books);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(GetBooks)}");
                Debug.WriteLine(ex.Message);
                return StatusCode(500);
            }
        }
        [HttpGet]
        [ActionName("GetAuthors")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAuthors()
        {
            try
            {
                DataService dataService = new DataService(_dataRepository);
                List<string> authors = await dataService.GetAuthors();
                return Ok(authors);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(GetAuthors)}");
                Debug.WriteLine(ex.Message);
                return StatusCode(500);
            }
        }
        [HttpPost]
        [ActionName("GetBookByIsbn")]
        public async Task<IActionResult> GetBooksByIsbn(string isbn)
        {
            try
            {
                DataService dataService = new DataService(_dataRepository);

                var book = await dataService.GetBookByIsbn(isbn);

                if (book != null)
                {
                    return Ok(book);
                }
                return BadRequest($"Book with isbn {isbn} not found.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(GetBooksByIsbn)}");
                Debug.WriteLine(ex.Message);
                return StatusCode(500);
            }
        }
        [HttpPost]
        [ActionName("BorrowBook")]
        public async Task<IActionResult> BorrowBook(string isbn)
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims?.FirstOrDefault(_ => _.Type == "Id")?.Value);

                DataService dataService = new DataService(_dataRepository);

                BookLoanModel activeLoan = await dataService.GetUserActiveLoan(userId);

                if (activeLoan != null)
                {
                    return BadRequest("You already have an active loan. Come back after you return it.");
                }

                var bookLoan = await dataService.BorrowBook(isbn, userId);
                if (bookLoan != null)
                {
                    return Ok(bookLoan);
                }

                return StatusCode(500, "Book not found or unavailable.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(BorrowBook)}");
                Debug.WriteLine(ex.Message);
                return StatusCode(500);
            }
        }
        [HttpPost]
        [ActionName("ReturnBook")]
        public async Task<IActionResult> ReturnBook()
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims?.FirstOrDefault(_ => _.Type == "Id")?.Value);

                DataService dataService = new DataService(_dataRepository);
                var result = await dataService.ReturnBook(userId);

                if (result != null)
                {
                    return Ok(result);
                }
                return BadRequest("You have no active loans.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(ReturnBook)}");
                Debug.WriteLine(ex.Message);
                return StatusCode(500);
            }
        }
        [HttpPost]
        [ActionName("AddNewBook")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AddBook(BookDto book)
        {
            try
            {
                DataService dataService = new DataService(_dataRepository);
                var insertedBook = await dataService.InsertBook(book);

                return Ok(insertedBook);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{nameof(AddBook)}");
                Debug.WriteLine(ex.Message);
                return StatusCode(500);
            }
        }
    }
}
