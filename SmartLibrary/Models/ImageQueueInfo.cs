using Wpf.Ui.Controls;

namespace SmartLibrary.Models
{
    public record class ImageQueueInfo
    {
        public required Image Image { get; set; }
        public required string Isbn { get; set; }
        public required string Url { get; set; }
    }
}
