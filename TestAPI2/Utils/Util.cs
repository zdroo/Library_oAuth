using Library_oAuth.Models;

namespace Library_oAuth.Utils
{
    public class Util
    {
        public static ConfigurationObject GetConfigurationObject()
        {
            return new ConfigurationObject()
            {
                ConnectionString = Environment.GetEnvironmentVariable("SQL_SERVER"),
                DataBaseName = "Library",
                SchemaName = "dbo",
                LibraryToken = "my super secret token"
            };
        }
    }
}
