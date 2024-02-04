namespace SmartLibrary.Models
{
    public record class BookInfo
    {
        public string Isbn { get; set; }
        public string BookName { get; set; }
        public uint ShelfNumber { get; set; }
        public bool IsBorrowed { get; set; }
        public string Author { get; set; }
        public string Press { get; set; }
        public string PressDate { get; set; }
        public string PressPlace { get; set; }
        public string PressText { get; set; }
        public float Price { get; set; }
        public string BookDesc { get; set; }
        public string Pages { get; set; }
        public string Words { get; set; }
        public Uri Picture { get; set; }

        public BookInfo(string isbn, string bookName, uint shelfNumber, bool isBorrowed, string author, string press, string pressDate, string pressPlace, string pressText, float price, string bookDesc, string pages, string words, Uri picture)
        {
            Isbn = isbn;
            BookName = bookName;
            ShelfNumber = shelfNumber;
            IsBorrowed = isBorrowed;
            Author = author;
            Press = press;
            PressDate = pressDate;
            PressPlace = pressPlace;
            PressText = pressText;
            Price = price;
            BookDesc = bookDesc;
            Pages = pages;
            Words = words;
            Picture = picture;
        }
    }
}
