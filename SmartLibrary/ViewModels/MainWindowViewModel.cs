using Shared.Helpers;
using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace SmartLibrary.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<object> _navigationItems = [];

        [ObservableProperty]
        private ObservableCollection<object> _navigationFooter = [];

        public MainWindowViewModel()
        {
            NavigationItems =
                [
                    new NavigationViewItem()
                    {
                        Content = "主页",
                        Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                        TargetPageType = typeof(Views.Pages.Home),
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
                ];

            if (SettingsHelper.GetBoolean("IsAdministrator"))
            {
                NavigationFooter.Add(new NavigationViewItem()
                {
                    Content = "管理",
                    Icon = new SymbolIcon { Symbol = SymbolRegular.Apps24 },
                    TargetPageType = typeof(Views.Pages.BookManage)
                });
            }


            NavigationFooter.Add(new NavigationViewItem()
            {
                Content = "蓝牙",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Bluetooth24 },
                TargetPageType = typeof(Views.Pages.BluetoothSettings)
            });

            NavigationFooter.Add(new NavigationViewItem()
            {
                Content = "设置",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.Settings)
            });
        }
    }
}