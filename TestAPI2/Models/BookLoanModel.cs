namespace Library_oAuth.Models
{
    public class BookLoanModel
    {
        public int UserId { get; set; }
        public string ItemId { get; set; }
        public decimal Price { get; set; }
        public decimal DelayCost { get; protected set; }
        public DateTime StartDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public bool IsActive { get; set; }
        public void ComputeDelayCost()//DateTime today)
        {
            DelayCost = Convert.ToInt32((DateTime.Now - ReturnDate).TotalDays) > 0 ? Convert.ToInt32((DateTime.Now - ReturnDate).TotalDays) * Price / 100 : 0;
        }
    }
}




