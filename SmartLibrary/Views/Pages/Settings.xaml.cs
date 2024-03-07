using SmartLibrary.Helpers;
using SmartLibrary.ViewModels;
using System.Reflection;
using Wpf.Ui.Controls;

namespace SmartLibrary.Views.Pages
{
    public partial class Settings : INavigableView<SettingsViewModel>
    {
        public SettingsViewModel ViewModel { get; }

        public Settings(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();

            AppVersion.Text = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
            DotNetVersion.Content = ".Net " + Environment.Version.ToString();
        }

        private void FileOccupancyExpander_Expanded(object sender, RoutedEventArgs e)
        {
            DataCount.Text = "数据库文件已占用 " + FileSize.GetFileSize(Environment.CurrentDirectory + @".\database\books.smartlibrary");
            CacheCount.Text = "缓存文件已占用 " + FileSize.GetDirectorySize(Environment.CurrentDirectory + @".\pictures\");
        }

        private void BooksDataButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", Environment.CurrentDirectory + @".\database\");
        }

        private void PictureCacheButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", Environment.CurrentDirectory + @".\pictures\");
        }
    }
}