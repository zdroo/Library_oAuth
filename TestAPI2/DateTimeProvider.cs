namespace Library_oAuth
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime GetToday()
        {
            return DateTime.UtcNow;
        }
    }
}
