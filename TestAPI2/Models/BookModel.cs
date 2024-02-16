namespace Library_oAuth.Models
{
    public class BookModel : ItemDataModel, IRental
    {
        public string Author { get; set; }
        public BookTypes BookType { get; set; } //demonstration purposes
        public int UserId { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public ItemTypes ItemType { get; set; } = ItemTypes.Book;
    }
}
