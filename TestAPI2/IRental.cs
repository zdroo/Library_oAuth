namespace Library_oAuth
{
    public interface IRental
    {
        int UserId { get; set; }
        decimal Price { get; set; }
        DateTime StartDate { get; set; }
        DateTime ReturnDate { get; set; }
        ItemTypes ItemType { get; set; }
    }
}
