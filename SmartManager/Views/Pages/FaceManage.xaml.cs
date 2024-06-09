using SamrtManager.ViewModels;
using Wpf.Ui.Controls;

namespace SamrtManager.Views.Pages
{
    public partial class FaceManage : INavigableView<FaceManageViewModel>
    {
        public FaceManageViewModel ViewModel { get; }

        public FaceManage(FaceManageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
