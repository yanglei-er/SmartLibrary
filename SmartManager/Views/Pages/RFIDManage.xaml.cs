using SmartManager.ViewModels;
using Wpf.Ui.Controls;

namespace SmartManager.Views.Pages
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
