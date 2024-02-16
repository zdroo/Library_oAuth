using Library_oAuth.Models;
using Library_oAuth.Utils;

namespace Library_oAuth.Queries
{
    public static class SqlQueries
    {
        public static ConfigurationObject ConfigurationObject = Util.GetConfigurationObject();

        public readonly static string GetAllBooks = $"select id, name, price, pieces, author from {ConfigurationObject.DataBaseName}.{ConfigurationObject.SchemaName}.Books where type = @type;";
        public readonly static string GetBookByIsbn = $"select id, name, price, pieces, type, author from {ConfigurationObject.DataBaseName}.{ConfigurationObject.SchemaName}.Books where id = @id;";
        public readonly static string UpdateBook = $"update {ConfigurationObject.DataBaseName}.{ConfigurationObject.SchemaName}.Books set name = @name, rentalprice = @rentalprice, pieces = @pieces where id = @id;";
        public readonly static string InsertUser = $"insert into {ConfigurationObject.DataBaseName}.{ConfigurationObject.SchemaName}.Users(email, passwordhash, passwordsalt, tokencreated, tokenexpires, role) values(@email, @passwordhash, @passwordsalt, @tokencreated, @tokenexpires, @role);";
        public readonly static string GetUserByEmail = $"select id, email, passwordhash, passwordsalt, refreshtoken, tokencreated, tokenexpires, role from {ConfigurationObject.DataBaseName}.{ConfigurationObject.SchemaName}.Users where email = @email;";
        public readonly static string UpdateUser = $"update {ConfigurationObject.DataBaseName}.{ConfigurationObject.SchemaName}.Users set refreshtoken = @refreshtoken, tokencreated = @tokencreated, tokenexpires = @tokenexpires where id = @id;";
        public readonly static string IncrementDecrementPieces = $"update {ConfigurationObject.DataBaseName}.{ConfigurationObject.SchemaName}.Books set pieces = pieces @operation 1 where id = @id;";
        public readonly static string InsertLoan = $"insert into {ConfigurationObject.DataBaseName}.{ConfigurationObject.SchemaName}.rentals(userid, itemid, price, startdate, returndate, isactive) values(@userid, @itemid, @price, @startdate, @returndate, @isactive)";
        public readonly static string GetLastLoanId = $"select max(id) from {ConfigurationObject.DataBaseName}.{ConfigurationObject.SchemaName}.loans;";
        public readonly static string GetUserActiveLoanByUserId = $"select userid, itemid, price, startdate, returndate, isactive from {ConfigurationObject.DataBaseName}.{ConfigurationObject.SchemaName}.rentals where isactive = 1 and userid = @userid;"; 
        public readonly static string SetLoanInactive = $"update {ConfigurationObject.DataBaseName}.{ConfigurationObject.SchemaName}.rentals set isactive = 0 where userid = @userid";
        public readonly static string InsertBook = $"insert into {ConfigurationObject.DataBaseName}.{ConfigurationObject.SchemaName}.Books(id, name, author, price, pieces, type) values(@id, @name, @author, @price, @pieces, @type);";
        public readonly static string GetBookIdByUserRental = $"select itemid from {ConfigurationObject.DataBaseName}.{ConfigurationObject.SchemaName}.rentals where userid = @userid";
        public readonly static string GetRentalItemById = $"select id, name, price, pieces, type, author from {ConfigurationObject.DataBaseName}.{ConfigurationObject.SchemaName}.Books where id = @id;";
        public readonly static string GetAllItems = $"select id, name, price, pieces, author from {ConfigurationObject.DataBaseName}.{ConfigurationObject.SchemaName}.Books;";
        public readonly static string GetCurrentLoanInactive = @$"select userid, itemid, price, startdate, returndate, isactive from
                                                                {ConfigurationObject.DataBaseName}.{ConfigurationObject.SchemaName}.rentals where userid = @userid";
        public readonly static string GetAuthors = $"select author from {ConfigurationObject.DataBaseName}.{ConfigurationObject.SchemaName}.Books";
    }
}
