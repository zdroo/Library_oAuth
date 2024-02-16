using Library_oAuth.Models;

namespace Library_oAuth.Repositories.Interfaces
{
    public interface IDataRepository
    {
        Task<IEnumerable<BookDto>> GetBooks();
        Task<BookModel> GetBookByIsbn(string isbns);
        //Task UpdateBooks(BookModel book);
        Task InsertUser(User user);
        Task<User> GetUserByEmail(string email);
        Task UpdateUser(User user);
        Task<BookLoanModel> BorrowBook(BookLoanModel bookLoan);
        Task<BookLoanModel> ReturnBook(BookLoanModel loan);
        Task<BookLoanModel> GetUserActiveLoan(int userId);
        Task<BookModel> InsertBook(BookDto book);
        Task<IEnumerable<string>> GetAuthors();
    }
}
