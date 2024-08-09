using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace SmartManager.ViewModels
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
                        Content = "用户",
                        Icon = new SymbolIcon { Symbol = SymbolRegular.People24 },
                        TargetPageType = typeof(Views.Pages.FaceManage)
                    }
                ];

            NavigationFooter.Add(new NavigationViewItem()
            {
                Content = "设置",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.Settings)
            });
        }
    }
}
