using System.Security.Cryptography.X509Certificates;
using Library_oAuth.Models;
using Library_oAuth.Repositories.Interfaces;

namespace Library_oAuth.Services
{
    public class DataService
    {
        IDataRepository _dataRepository;
        IDateTimeProvider _dateTimeProvider;
        public DataService(IDataRepository dataRepository)//, IDateTimeProvider dateTimeProvider)
        {
            _dataRepository = dataRepository;
            //_dateTimeProvider = dateTimeProvider;
        }
        public async Task<List<BookDto>> GetBooks()
        {
            var books = await _dataRepository.GetBooks();
            return books.ToList();
        }
        public async Task<List<string>> GetAuthors()
        {
            var authors = await _dataRepository.GetAuthors();
            return authors.ToList();
        }
        public async Task<BookModel> GetBookByIsbn(string isbn)
        {
            return await _dataRepository.GetBookByIsbn(isbn);
        }
        public BookLoanModel CreateLoan(BookModel book, int userId)
        {
            BookLoanModel loan;

            if (book != null && book.Pieces > 0)
            {
                loan = new BookLoanModel
                {
                    ItemId = book.Id,
                    Price = book.Price,
                    StartDate = DateTime.Now,
                    ReturnDate = DateTime.Now.AddDays(14),
                    UserId = userId,
                };
                return loan;
            }
            return null;
        }
        public async Task<BookLoanModel> BorrowBook(string isbn, int userId)
        {
            BookModel book = await _dataRepository.GetBookByIsbn(isbn); //better to interogate the db since someone can borrow right after current user runs GetBooks()
            BookLoanModel loan = CreateLoan(book, userId);

            if (loan == null) { return null; }

            return await _dataRepository.BorrowBook(loan); ;
        }
        public async Task<BookLoanModel> ReturnBook(int userId)
        {
            BookLoanModel loan = await _dataRepository.GetUserActiveLoan(userId);
            if (loan == null) { return null; }

            var inactiveLoan = await _dataRepository.ReturnBook(loan);

            inactiveLoan.ComputeDelayCost();
            return inactiveLoan;
        }
        public async Task<BookModel> InsertBook(BookDto book)
        {
            return await _dataRepository.InsertBook(book);
        }
        public async Task<BookLoanModel> GetUserActiveLoan(int userId)
        {
            return await _dataRepository.GetUserActiveLoan(userId);
        }
        public async Task InsertUser(User user)
        {
            await _dataRepository.InsertUser(user);
        }
        public async Task<User> GetUserByEmail(string email)
        {
            return await _dataRepository.GetUserByEmail(email);
        }
        public async Task UpdateUser(User user)
        {
            await _dataRepository.UpdateUser(user);
        }

    }
}
