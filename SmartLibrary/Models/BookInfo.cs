namespace SmartLibrary.Models
{
    public record class BookInfo
    {
        public string Isbn { get; set; }
        public string BookName { get; set; }
        public string Author { get; set; }
        public string? Press { get; set; }
        public string? PressDate { get; set; }
        public string? PressPlace { get; set; }
        public string? ClcName { get; set; }
        public string? Price { get; set; }
        public string? BookDesc { get; set; }
        public string? Pages { get; set; }
        public string? Words { get; set; }
        public string? Language { get; set; }
        public string? Picture { get; set; }
        public long ShelfNumber { get; set; }
        public bool IsBorrowed { get; set; }

        public BookInfo()
        {
            Isbn = string.Empty;
            BookName = string.Empty;
            Author = string.Empty;
        }

        public BookInfo(string isbn, string bookName, string author, string press, string pressDate, string pressPlace, string price, string clcName, string bookDesc, string pages, string words, string language, long shelfNumber, bool isBorrowed, string picture)
        {
            Isbn = isbn;
            BookName = bookName;
            ShelfNumber = shelfNumber;
            IsBorrowed = isBorrowed;
            Author = author;
            Press = press;
            PressDate = pressDate;
            PressPlace = pressPlace;
            ClcName = clcName;
            Price = price;
            BookDesc = bookDesc;
            Pages = pages;
            Words = words;
            Picture = picture;
            Language = language;
        }
    }
}
