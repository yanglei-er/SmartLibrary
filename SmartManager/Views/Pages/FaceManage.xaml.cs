using SmartManager.ViewModels;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace SmartManager.Views.Pages
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

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.GotoTargetPage(textBox.Text);
                XuNiBox.Focus();
            }
        }

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                XuNiBox.Focus();
            }
        }

        private void Page_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
                Array a = (Array)e.Data.GetData(DataFormats.FileDrop);
                foreach (string filepath in a)
                {
                    if (Directory.Exists(filepath))
                    {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                        return;
                    }
                    if (!filepath.EndsWith("smartmanager"))
                    {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                        return;
                    }
                }
                e.Effects = DragDropEffects.Link;
            }
            else
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void Page_Drop(object sender, DragEventArgs e)
        {
            List<string> files = new((string[])e.Data.GetData(DataFormats.FileDrop));
            ViewModel.DropFileImportAsync(files);
        }
    }
}
