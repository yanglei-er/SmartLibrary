namespace SmartLibrary.Models
{
    public record class BookInfoSimple
    {
        public string Isbn { get; set; }
        public string BookName { get; set; }
        public string? Author { get; set; }
        public Int64 ShelfNumber { get; set; }
        public bool IsBorrowed { get; set; }

        public BookInfoSimple(string isbn, string bookName, string? author, Int64 shelfNumber, bool isBorrowed)
        {
            Isbn = isbn;
            BookName = bookName;
            Author = author;
            ShelfNumber = shelfNumber;
            IsBorrowed = isBorrowed;
        }
    }
}
