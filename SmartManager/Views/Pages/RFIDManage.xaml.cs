using SamrtManager.ViewModels;
using Wpf.Ui.Controls;

namespace SamrtManager.Views.Pages
{
    public partial class RFIDManage : INavigableView<RFIDManageViewModel>
    {
        public RFIDManageViewModel ViewModel { get; }

        public RFIDManage(RFIDManageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
