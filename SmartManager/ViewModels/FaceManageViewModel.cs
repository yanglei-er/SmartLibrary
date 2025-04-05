using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using Shared.Helpers;
using Shared.Models;
using SmartManager.Helpers;
using SmartManager.Models;
using SmartManager.Views.Pages;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace SmartManager.ViewModels
{
    public partial class FaceManageViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly ISnackbarService _snackbarService;
        private readonly IContentDialogService _contentDialogService;
        private UsersDb FacesDb = UsersDb.GetDatabase("faces.smartmanager");
        private int TotalPageCount;
        private bool needRefresh = false;

        [ObservableProperty]
        private string _autoSuggestBoxText = string.Empty;

        [ObservableProperty]
        private bool _missingDatabase = false;

        [ObservableProperty]
        private bool _databaseEmpty = false;

        [ObservableProperty]
        private bool _isTopbarEnabled = true;

        [ObservableProperty]
        private bool _isBottombarEnabled = true;

        [ObservableProperty]
        private bool _isDelButtonEnabled = false;

        [ObservableProperty]
        private List<int> _pageCountList = [20, 30, 50, 80];

        [ObservableProperty]
        private DataView _dataGridItems = new();

        [ObservableProperty]
        private int _totalCount = 0;

        [ObservableProperty]
        private int _displayIndex = SettingsHelper.GetInt("FaceManageDisplayIndex");

        [ObservableProperty]
        private ObservableCollection<PageButton> _pageButtonList = [];

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _targetPage = 1;

        [ObservableProperty]
        private bool _isPageUpEnabled = false;

        [ObservableProperty]
        private bool _isPageDownEnabled = false;

        [ObservableProperty]
        private bool _isFlyoutOpen = false;

        [ObservableProperty]
        private string _flyoutText = string.Empty;

        public FaceManageViewModel(INavigationService navigationService, ISnackbarService snackbarService, IContentDialogService contentDialogService)
        {
            _navigationService = navigationService;
            _snackbarService = snackbarService;
            _contentDialogService = contentDialogService;

            WeakReferenceMessenger.Default.Register<string, string>(this, "FaceManage", (_, _) => needRefresh = true);

            if (UsersDb.IsDatabaseConnected("faces.smartmanager"))
            {
                needRefresh = true;
            }
            else
            {
                MissingDatabase = true;
                IsTopbarEnabled = false;
                IsBottombarEnabled = false;
            }
        }

        public Task OnNavigatedToAsync()
        {
            if (needRefresh)
            {
                if (string.IsNullOrEmpty(AutoSuggestBoxText))
                {
                    RefreshAsync();
                    PagerAsync();
                }
                else
                {
                    AutoSuggest(AutoSuggestBoxText);
                }
                needRefresh = false;
            }
            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync()
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private void RefreshDatabase()
        {
            FacesDb = UsersDb.GetDatabase("faces.smartmanager");
            if (UsersDb.IsDatabaseConnected("faces.smartmanager"))
            {
                MissingDatabase = false;
                IsTopbarEnabled = true;
                RefreshAsync();
                PagerAsync();
            }
        }

        [RelayCommand]
        private async Task CreateDatabase()
        {
            await FacesDb.CreateDataBaseAsync();
            FacesDb = UsersDb.GetDatabase("faces.smartmanager");
            MissingDatabase = false;
            IsTopbarEnabled = true;
            RefreshAsync();
        }

        [RelayCommand]
        private void AddFace()
        {
            _navigationService.NavigateWithHierarchy(typeof(AddFace));
        }

        [RelayCommand]
        private void EditFace(DataRowView selectedItem)
        {
            _navigationService.NavigateWithHierarchy(typeof(EditFace));
            string uid = (string)selectedItem[0];
            WeakReferenceMessenger.Default.Send(uid, "EditFace");
        }

        private async void RefreshAsync()
        {
            TotalCount = await FacesDb.GetRecordCountAsync();
            if (TotalCount == 0)
            {
                DatabaseEmpty = true;
                IsBottombarEnabled = false;
                TotalPageCount = 0;
                return;
            }
            else
            {
                DatabaseEmpty = false;
                IsBottombarEnabled = true;
            }
            TotalPageCount = TotalCount / PageCountList[DisplayIndex] + ((TotalCount % PageCountList[DisplayIndex]) == 0 ? 0 : 1);
            if (CurrentPage > TotalPageCount) CurrentPage = TotalPageCount;
            if (TotalPageCount == 1) { IsPageUpEnabled = false; IsPageDownEnabled = false; return; }
            if (CurrentPage != 1) { IsPageUpEnabled = true; }
            if (CurrentPage != TotalPageCount) { IsPageDownEnabled = true; }
        }

        private async void PagerAsync()
        {
            IsDelButtonEnabled = false;
            DataGridItems = (await FacesDb.ExecutePagerSimpleAsync(CurrentPage, PageCountList[DisplayIndex])).DefaultView;

            PageButtonList.Clear();
            if (TotalPageCount <= 7)
            {
                for (int i = 1; i <= TotalPageCount; i++)
                {
                    PageButtonList.Add(new PageButton(i.ToString(), CurrentPage == i));
                }
            }
            else
            {
                if (CurrentPage <= 4)
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        PageButtonList.Add(new PageButton(i.ToString(), CurrentPage == i));
                    }
                    PageButtonList.Add(new PageButton("...", false, false));
                    PageButtonList.Add(new PageButton(TotalPageCount.ToString()));
                }
                else if (CurrentPage >= TotalPageCount - 3)
                {
                    PageButtonList.Add(new PageButton("1"));
                    PageButtonList.Add(new PageButton("...", false, false));
                    PageButtonList.Add(new PageButton((TotalPageCount - 4).ToString(), CurrentPage == TotalPageCount - 4));
                    PageButtonList.Add(new PageButton((TotalPageCount - 3).ToString(), CurrentPage == TotalPageCount - 3));
                    PageButtonList.Add(new PageButton((TotalPageCount - 2).ToString(), CurrentPage == TotalPageCount - 2));
                    PageButtonList.Add(new PageButton((TotalPageCount - 1).ToString(), CurrentPage == TotalPageCount - 1));
                    PageButtonList.Add(new PageButton(TotalPageCount.ToString(), CurrentPage == TotalPageCount));
                }
                else
                {
                    PageButtonList.Add(new PageButton("1"));
                    PageButtonList.Add(new PageButton("...", false, false));
                    PageButtonList.Add(new PageButton((CurrentPage - 1).ToString()));
                    PageButtonList.Add(new PageButton(CurrentPage.ToString(), true));
                    PageButtonList.Add(new PageButton((CurrentPage + 1).ToString()));
                    PageButtonList.Add(new PageButton("...", false, false));
                    PageButtonList.Add(new PageButton(TotalPageCount.ToString()));
                }
            }
        }

        partial void OnDisplayIndexChanged(int value)
        {
            SettingsHelper.SetConfig("FaceManageDisplayIndex", value.ToString());
            RefreshAsync();
            if (CurrentPage == 1) PagerAsync();
            CurrentPage = 1;
        }

        [RelayCommand]
        private void OnTopButtonClick(string parameter)
        {
            if (parameter == "Import")
            {
                OpenFileDialog openFileDialog = new()
                {
                    Title = "导入数据库",
                    Filter = "人脸数据库 (*.smartmanager)|*.smartmanager",
                    Multiselect = true,
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    List<string> fileNames = new(openFileDialog.FileNames);
                    ImportDatabase(fileNames);
                }
            }
            else if (parameter == "Export")
            {
                SaveFileDialog saveFileDialog = new()
                {
                    Title = "导出数据库",
                    FileName = "智慧管理员" + DateTime.Now.ToString("yyyyMMdd-HHmmss"),
                    Filter = "SmartManager数据库 (*.smartmanager)|*.smartmanager",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    System.Media.SystemSounds.Asterisk.Play();
                    File.Copy(@".\database\faces.smartmanager", saveFileDialog.FileName, true);
                    _snackbarService.Show("导出数据库", $"{Path.GetFileName(saveFileDialog.FileName)} 已导出至 {Path.GetDirectoryName(saveFileDialog.FileName)} 下", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
                }
            }
        }

        public async void DropFileImportAsync(List<string> files)
        {
            ContentDialogResult result = await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
            {
                Title = "导入数据库",
                Content = $"是否导入你选择的 {files.Count} 个数据库?",
                PrimaryButtonText = "是",
                CloseButtonText = "否",
            });

            if (result == ContentDialogResult.Primary)
            {
                ImportDatabase(files);
            }
        }

        private async void ImportDatabase(List<string> files)
        {
            if (!UsersDb.IsDatabaseConnected("faces.smartmanager"))
            {
                await CreateDatabase();
            }
            int[] mergedResult = [0, 0];
            List<string> repeatFileNames = [];

            foreach (string fileName in files)
            {
                if (fileName == Path.GetFullPath(@".\database\faces.smartmanager")) //避免重复
                {
                    repeatFileNames.Add(fileName);
                    continue;
                }
                int[] _mergedResult = await FacesDb.MergeDatabaseAsync(fileName);
                mergedResult[0] += _mergedResult[0];
                mergedResult[1] += _mergedResult[1];
            }

            if (mergedResult[0] != 0)
            {
                RefreshAsync();
                PagerAsync();
            }

            _snackbarService.Show("导入数据库", $"{files.Count} 个数据库已导入，共 {mergedResult[0] + mergedResult[1]} 条数据，导入 {mergedResult[0]} 条，重复 {mergedResult[1]} 条。", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));
        }

        [RelayCommand]
        private void OnPageButtonClick(string parameter)
        {
            if (parameter == "PageUp")
            {
                if (CurrentPage > 1)
                {
                    CurrentPage--;
                    if (!IsPageDownEnabled) IsPageDownEnabled = true;
                }
            }
            else
            {
                if (CurrentPage < TotalPageCount)
                {
                    CurrentPage++;
                    if (!IsPageUpEnabled) IsPageUpEnabled = true;
                }
            }
        }

        partial void OnCurrentPageChanged(int value)
        {
            TargetPage = value;
            PagerAsync();
            if (CurrentPage == 1) IsPageUpEnabled = false;
            else if (CurrentPage == TotalPageCount) IsPageDownEnabled = false;
        }

        [RelayCommand]
        private void GotoPage(string page)
        {
            CurrentPage = int.Parse(page);
            if (CurrentPage > 1) IsPageUpEnabled = true;
            if (CurrentPage < TotalPageCount) IsPageDownEnabled = true;
        }

        [RelayCommand]
        partial void OnTargetPageChanged(int value)
        {
            if (value > TotalPageCount)
            {
                FlyoutText = $"输入页码超过最大页码！";
                IsFlyoutOpen = true;
                TargetPage = TotalPageCount;
            }
            else if (value == 0)
            {
                FlyoutText = "最小页码为 1 ";
                IsFlyoutOpen = true;
                TargetPage = 1;
            }
            else if (value > 0 && value < TotalPageCount)
            {
                if (IsFlyoutOpen)
                {
                    IsFlyoutOpen = false;
                }
            }
        }

        public void GotoTargetPage(string page)
        {
            if (string.IsNullOrEmpty(page))
            {
                TargetPage = -1;
                TargetPage = CurrentPage;
            }
            else
            {
                CurrentPage = TargetPage;
                if (CurrentPage > 1) IsPageUpEnabled = true;
                if (CurrentPage < TotalPageCount) IsPageDownEnabled = true;
            }
            if (IsFlyoutOpen)
            {
                IsFlyoutOpen = false;
            }
        }

        [RelayCommand]
        private async Task DelFaces(IList selectedItems)
        {
            System.Media.SystemSounds.Asterisk.Play();
            ContentDialogResult result = await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
            {
                Title = "删除数据",
                Content = $"是否删除你选择的 {selectedItems.Count} 个人脸数据，此操作不可撤销！",
                PrimaryButtonText = "是",
                CloseButtonText = "否",
            });

            if (result == ContentDialogResult.Primary)
            {
                List<int> indexs = [];
                if (DataGridItems.Table != null)
                {
                    foreach (DataRowView item in selectedItems)
                    {
                        string uid = (string)item[0];
                        FacesDb.DelFaceAsync(uid);
                        if (!IsBottombarEnabled)
                        {
                            indexs.Add(DataGridItems.Table.Rows.IndexOf(item.Row));
                        }
                    }
                }

                _snackbarService.Show("删除数据", $"已删除你选择的 {selectedItems.Count} 个人脸数据", ControlAppearance.Success, new SymbolIcon(SymbolRegular.Info16), TimeSpan.FromSeconds(3));

                if (IsBottombarEnabled)
                {
                    RefreshAsync();
                    PagerAsync();
                }
                else
                {
                    if (DataGridItems.Table != null)
                    {
                        foreach (int index in indexs)
                        {
                            DataGridItems.Table.Rows.RemoveAt(index);
                        }
                        DataGridItems.Table.AcceptChanges();
                    }
                    if (DataGridItems.Count == 0)
                    {
                        DatabaseEmpty = true;
                    }
                    TotalCount = DataGridItems.Count;
                }
            }
        }

        [RelayCommand]
        private void DelOneFace(DataRowView selectedItem)
        {
            string uid = (string)selectedItem[0];
            FacesDb.DelFaceAsync(uid);
            if (IsBottombarEnabled)
            {
                RefreshAsync();
                PagerAsync();
            }
            else
            {
                if (DataGridItems.Table != null)
                {
                    DataGridItems.Table.Rows.Remove(selectedItem.Row);
                    DataGridItems.Table.AcceptChanges();
                }
                if (DataGridItems.Count == 0)
                {
                    DatabaseEmpty = true;
                }
                TotalCount--;
            }
        }

        public void UpdateSimple(FaceInfoSimple faceInfo)
        {
            FacesDb.UpdateSimpleAsync(faceInfo.Uid, faceInfo.Name, faceInfo.Sex, faceInfo.Age, faceInfo.JoinTime);
        }

        partial void OnAutoSuggestBoxTextChanged(string value)
        {
            AutoSuggest(value);
        }

        private async void AutoSuggest(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                IsBottombarEnabled = false;

                DataGridItems = (await FacesDb.AutoSuggestByNameAsync(value)).DefaultView;

                if (DataGridItems.Count > 0)
                {
                    DatabaseEmpty = false;
                }
                else
                {
                    DatabaseEmpty = true;
                }
                TotalCount = DataGridItems.Count;
            }
            else
            {
                RefreshAsync();
                PagerAsync();
            }
        }
    }
}
