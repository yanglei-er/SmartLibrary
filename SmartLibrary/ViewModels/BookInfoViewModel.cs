using CommunityToolkit.Mvvm.Messaging;

namespace SmartLibrary.ViewModels
{
    public partial class BookInfoViewModel : ObservableObject
    {
        public BookInfoViewModel()
        {
            WeakReferenceMessenger.Default.Register<string, string>(this, "BookInfo", OnMessageReceived);
        }

        private void OnMessageReceived(object recipient, string message)
        {
            MessageBox.Show(message);
        }
    }
}