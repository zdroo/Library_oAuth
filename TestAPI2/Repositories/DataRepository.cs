using Dapper;
using System.Data.SqlClient;
using Library_oAuth.Models;
using Library_oAuth.Queries;
using Library_oAuth.Repositories.Interfaces;
using Library_oAuth.Utils;

namespace Library_oAuth.Repositories
{
    public class DataRepository : IDataRepository
    {
        readonly ConfigurationObject configurationObject = Util.GetConfigurationObject();
        public async Task<IEnumerable<BookDto>> GetBooks()
        {
            using (SqlConnection connection = new SqlConnection(configurationObject.ConnectionString))
            {
                return await connection.QueryAsync<BookDto>(SqlQueries.GetAllBooks, new { type = ItemTypes.Book });
            }
        }
        public async Task<BookModel> GetBookByIsbn(string isbn)
        {
            using (SqlConnection connection = new SqlConnection(configurationObject.ConnectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<BookModel>(SqlQueries.GetBookByIsbn, new { id = isbn });
            }
        }
        public async Task InsertUser(User user)
        {
            using (SqlConnection connection = new SqlConnection(configurationObject.ConnectionString))
            {
                await connection.ExecuteAsync(SqlQueries.InsertUser, new { user.Email, user.PasswordHash, user.PasswordSalt, user.TokenCreated, user.TokenExpires, user.Role });
            }
        }
        public async Task<User> GetUserByEmail(string email)
        {
            User user;
            using (SqlConnection connection = new SqlConnection(configurationObject.ConnectionString))
            {
                user = await connection.QueryFirstOrDefaultAsync<User>(SqlQueries.GetUserByEmail, new { email });
            }
            return user;
        }
        public async Task UpdateUser(User user)
        {
            using (SqlConnection connection = new SqlConnection(configurationObject.ConnectionString))
            {
                await connection.ExecuteAsync(SqlQueries.UpdateUser, new { user.RefreshToken, user.TokenCreated, user.TokenExpires, user.Id });
            }
        }
        public async Task<BookLoanModel> BorrowBook(BookLoanModel bookLoan)
        {
            using (SqlConnection connection = new SqlConnection(configurationObject.ConnectionString))
            {
                await connection.ExecuteAsync(SqlQueries.IncrementDecrementPieces.Replace("@operation", "-"), new { id = bookLoan.ItemId });
                await connection.ExecuteAsync(SqlQueries.InsertLoan, new { userid = bookLoan.UserId, itemid = bookLoan.ItemId, price = bookLoan.Price, startdate = bookLoan.StartDate, returndate = bookLoan.ReturnDate, isactive = 1 });
                return await connection.QueryFirstOrDefaultAsync<BookLoanModel>(SqlQueries.GetUserActiveLoanByUserId, new { userid = bookLoan.UserId });
            }
        }
        public async Task<BookLoanModel> ReturnBook(BookLoanModel loan)
        {
            using (SqlConnection connection = new SqlConnection(configurationObject.ConnectionString))
            {
                string itemId = await connection.QueryFirstOrDefaultAsync<string>(SqlQueries.GetBookIdByUserRental, new {userid = loan.UserId});
                await connection.ExecuteAsync(SqlQueries.IncrementDecrementPieces.Replace("@operation", "+"), new { id = itemId});
                await connection.ExecuteAsync(SqlQueries.SetLoanInactive, new { userid = loan.UserId });
                return await connection.QueryFirstOrDefaultAsync<BookLoanModel>(SqlQueries.GetCurrentLoanInactive, new { userid = loan.UserId });
                // TO DO - insert extracost
            }
        }
        public async Task<BookLoanModel> GetUserActiveLoan(int userId)
        {
            using (SqlConnection connection = new SqlConnection(configurationObject.ConnectionString))
            {
                return await connection.QueryFirstOrDefaultAsync<BookLoanModel>(SqlQueries.GetUserActiveLoanByUserId, new { userId });
            }
        }
        public async Task<BookModel> InsertBook(BookDto book)
        {
            using (SqlConnection connection = new SqlConnection(configurationObject.ConnectionString))
            {
                await connection.ExecuteAsync(SqlQueries.InsertBook, new { id = book.Id, name = book.Name, author = book.Author, price = book.Price, pieces = book.Pieces, type = ItemTypes.Book });
                return await connection.QueryFirstOrDefaultAsync<BookModel>(SqlQueries.GetBookByIsbn, new { id = book.Id });
            }
        }
        public async Task<IEnumerable<string>> GetAuthors()
        {
            using (SqlConnection connection = new SqlConnection(configurationObject.ConnectionString))
            {
                return await connection.QueryAsync<string>(SqlQueries.GetAuthors);
            }
        }
    }
}
