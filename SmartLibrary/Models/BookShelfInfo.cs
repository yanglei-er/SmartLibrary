namespace SmartLibrary.Models
{
    public record class BookShelfInfo
    {
        public string Isbn { get; set; }
        public string BookName { get; set; }
        public string Author { get; set; }
        public string Press { get; set; }
        public string Picture { get; set; }

        public BookShelfInfo(string isbn, string bookName, string author, string press, string picture)
        {
            Isbn = isbn;
            BookName = bookName;
            Author = author;
            Press = press;
            Picture = picture;
        }
    }
}
