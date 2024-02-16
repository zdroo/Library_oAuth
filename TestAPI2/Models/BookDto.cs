namespace Library_oAuth.Models
{
    public class BookDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int Pieces { get; set; }
        public decimal Price { get; set; }
    }
}
