using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ImageCleaner
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        const string cPickerFolder = "PickedFolderToken";
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(cPickerFolder, folder);
                this.TxtSelectedFolder.Text = folder.Path;
            }
            else
            {
                this.TxtSelectedFolder.Text = "Operation cancelled.";
            }
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            var folderPath = TxtSelectedFolder.Text;
            bool includeSubfolders = ChkIncludeSubfolders.IsChecked ?? false;

            //StorageApplicationPermissions.FutureAccessList.
            //if (!Directory.Exists(folderPath))
            //{
            //    DisplayFolderNotFoundAsync();

            //    return;
            //}

            if (!StorageApplicationPermissions.FutureAccessList.ContainsItem(cPickerFolder))
            {
                DisplayNoAccessDialogAsync();

                return;
            }

            var jpgExtensions = new[] { ".jpg" };
            

            var jpgFiles = await GetFilesAsync(jpgExtensions, includeSubfolders);

            var rawExtensions = new[] { ".3fr",
".ari", ".arw",
".bay",
".braw", ".crw", ".cr2", ".cr3",
".cap",
".data", ".dcs", ".dcr", ".dng",
".drf",
".eip", ".erf",
".fff",
".gpr",
".iiq",
".k25", ".kdc",
".mdc", ".mef", ".mos", ".mrw",
".nef", ".nrw",
".obm", ".orf",
".pef", ".ptx", ".pxn",
".r3d", ".raf", ".raw", ".rwl", ".rw2", ".rwz",
".sr2", ".srf", ".srw",
".tif",
".x3f" };

            var rawFiles = await GetFilesAsync(rawExtensions, includeSubfolders);

            var filesToDelete = GetRawFilesWhereJpgIsMissing(rawFiles, jpgFiles);

            LstItemsToDelete.ItemsSource = filesToDelete;
        }

        private IEnumerable<StorageFile> GetRawFilesWhereJpgIsMissing(IEnumerable<StorageFile> rawFiles, IEnumerable<StorageFile> jpgFiles)
        {
            List<StorageFile> rawsToDelete = new List<StorageFile>();
            
            // Iterate over all rawfiles and check if correlating jpg exists.
            foreach (var rawFile in rawFiles)
            {
                var name = Path.GetFileNameWithoutExtension(rawFile.Path);
                var file = jpgFiles.FirstOrDefault(d => Path.GetFileNameWithoutExtension(d.Path) == name);

                if (file == null)
                {
                    rawsToDelete.Add(rawFile);
                }
            }

            return rawsToDelete;
        }

        private static async Task<IEnumerable<StorageFile>> GetFilesAsync(string[] fileExtensions, bool includeSubfolders)
        {
            var folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(cPickerFolder);

            IReadOnlyList<StorageFile> files;
            
            if (includeSubfolders)
                files = await folder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName);
            else
                files = await folder.GetFilesAsync();


            var filteredFiles = files.Where(file => fileExtensions.Any(file.Name.ToLower().EndsWith));

            return filteredFiles;
        }

        private static IEnumerable<string> GetFilesFromDirectory(string folderPath, string[] jpgExtensions)
        {
            

            var files = Directory
                            .GetFiles(folderPath)
                            .Where(file => jpgExtensions.Any(file.ToLower().EndsWith))
                            .ToList();

            return files;
        }

        private async void DisplayFolderNotFoundAsync()
        {
            ContentDialog noWifiDialog = new ContentDialog()
            {
                Title = "Folder not found",
                Content = "Specified folder not found.",
                CloseButtonText = "Ok"
            };

            await noWifiDialog.ShowAsync();
        }

        private async void DisplayNoAccessDialogAsync()
        {
            ContentDialog noWifiDialog = new ContentDialog()
            {
                Title = "No access to this folder",
                Content = "Please select folder by using the filepicker.",
                CloseButtonText = "Ok"
            };

            await noWifiDialog.ShowAsync();
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (LstItemsToDelete.SelectedItems.Count > 0)
            {
                foreach (var item in LstItemsToDelete.SelectedItems)
                {
                    await (item as StorageFile).DeleteAsync();
                }
            }
        }

        private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            LstItemsToDelete.SelectAll();
        }
    }
}
