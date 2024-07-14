using SmartManager.Models;
using SmartManager.ViewModels;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace SmartManager.Views.Pages
{
    public partial class FaceManage : INavigableView<FaceManageViewModel>
    {
        private readonly FaceInfoSimple faceInfo = new(string.Empty, string.Empty, string.Empty, string.Empty);

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

        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (DataGrid.SelectedItem == null) { DelButton.IsEnabled = false; }
            else { DelButton.IsEnabled = true; }
        }

        private void DataGrid_BeginningEdit(object sender, System.Windows.Controls.DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Item is DataRowView dataRowView)
            {
                faceInfo.Name = (string)dataRowView[0];
                faceInfo.Sex = dataRowView[1].ToString();
                faceInfo.Age = dataRowView[2].ToString();
                faceInfo.JoinTime = dataRowView[3].ToString();
            }
        }

        private void DataGrid_RowEditEnding(object sender, System.Windows.Controls.DataGridRowEditEndingEventArgs e)
        {
            if (e.Row.Item is DataRowView dataRowView)
            {
                if (string.IsNullOrEmpty(dataRowView[0].ToString()))
                {
                    dataRowView[1] = faceInfo.Name;
                }

                if ((string)dataRowView[0] != faceInfo.Name || dataRowView[1].ToString() != faceInfo.Sex || dataRowView[2].ToString() != faceInfo.Age || dataRowView[3].ToString() != faceInfo.JoinTime)
                {
                    faceInfo.Name = (string)dataRowView[0];
                    faceInfo.Sex = dataRowView[1].ToString();
                    faceInfo.Age = dataRowView[2].ToString();
                    faceInfo.JoinTime = dataRowView[3].ToString();
                    ViewModel.UpdateSimple(faceInfo);
                }
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
