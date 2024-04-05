namespace SmartLibrary.Models
{
    public class PageButton
    {
        public string Name { get; init; }
        public bool IsCurrentPage { get; init; }
        public bool IsEnabled { get; init; }

        public PageButton(string name, bool isCurrentPage = false, bool isEnabled = true)
        {
            Name = name;
            IsCurrentPage = isCurrentPage;
            IsEnabled = isEnabled;
        }
    }
}