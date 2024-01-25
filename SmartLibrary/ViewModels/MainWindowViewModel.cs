using System.Collections.ObjectModel;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace SmartLibrary.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private ObservableCollection<object> _navigationItems = new();

        [ObservableProperty]
        private ObservableCollection<object> _navigationFooter = new();

        public MainWindowViewModel(INavigationService navigationService)
        {
            if (!_isInitialized)
            {
                InitializeViewModel();
            }
        }

        private void InitializeViewModel()
        {
            NavigationItems = new ObservableCollection<object>
            {
                new NavigationViewItem()
                {
                    Content = "主页",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                    TargetPageType = typeof(Views.Pages.Home),
                    NavigationCacheMode=NavigationCacheMode.Enabled
                },
                new NavigationViewItem()
                {
                    Content = "书架",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.Library24 },
                    TargetPageType = typeof(Views.Pages.Bookshelf)
                },
                new NavigationViewItem()
                {
                    Content = "借还",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.CheckboxCheckedSync20 },
                    TargetPageType = typeof(Views.Pages.Borrow_Return_Book)
                },
                new NavigationViewItem()
                {
                    Content = "信息",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.BookInformation24 },
                    TargetPageType = typeof(Views.Pages.BookInfo)
                }
            };

            NavigationFooter = new ObservableCollection<object>
            {
                new NavigationViewItem()
                {
                    Content = "蓝牙",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.Bluetooth24 },
                    TargetPageType = typeof(Views.Pages.BluetoothSettings)
                },
                new NavigationViewItem()
                {
                    Content = "设置",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                    TargetPageType = typeof(Views.Pages.Settings)
                }
            };

            _isInitialized = true;
        }
    }
}
