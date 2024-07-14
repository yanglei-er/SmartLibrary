using SmartManager.ViewModels;
using Wpf.Ui.Controls;

namespace SmartManager.Views.Pages
{
    public partial class EditFace : INavigableView<EditFaceViewModel>
    {
        public EditFaceViewModel ViewModel { get; set; }
        public EditFace(EditFaceViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }
    }
}
