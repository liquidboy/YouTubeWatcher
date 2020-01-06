using MediaLibraryLegacy.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls;
using SharedCode.SQLite;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

namespace MediaLibraryLegacy
{
    public sealed partial class MediaLibrary : UserControl
    {
        string mediaPath;

        public event EventHandler<PlayMediaEventArgs> OnPlayMedia;
        public event EventHandler OnMediaDeleted;

        public MediaLibrary()
        {
            this.InitializeComponent();
        }

        public void InitialSetup(string mediapath)
        {
            mediaPath = mediapath;
            Show();
            tbMediaDirectory.Text = mediapath;
        }

        private void ShowMediaFolder(object sender, RoutedEventArgs e) => OpenMediaFolder();

        private async void OpenMediaFolder()
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(mediaPath);
            await Launcher.LaunchFolderAsync(folder);
        }

        public void Show()
        {
            //wvMain.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
            grdLibrary.Visibility = Visibility.Visible;
            LoadLibraryItems(true);
            //ShowHideMediaPlayer(false);
        }

        public void Hide()
        {
            //wvMain.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
            grdLibrary.Visibility = Visibility.Collapsed;
            LoadLibraryItems(false);
            //ShowHideMediaPlayer(false);
        }

        private void LoadLibraryItems(bool load)
        {
            if (load)
            {
                icLibraryItems.ItemsSource = EntitiesHelper.RetrieveMediaMetadataAsViewCollection(mediaPath);
            }
            else
            {
                //icLibraryItems.Items.Clear();
                icLibraryItems.ItemsSource = null;
            }
        }

        private void PlayMedia(object sender, RoutedEventArgs e)
        {
            var but = sender as Button;
            if (but.DataContext is ViewMediaMetadata)
            {
                var vmd = (ViewMediaMetadata)but.DataContext;
                OnPlayMedia?.Invoke(null, new PlayMediaEventArgs() { ViewMediaMetadata = vmd });
            }
        }

        private void OnPlaylistSelected(object sender, EventArgs e)
        {
            XamlHelper.CloseFlyout(sender);
            if (e is PlaylistSelectedEventArgs && sender is FrameworkElement)
            {
                var uie = (FrameworkElement)sender;
                if (uie != null && uie.DataContext is ViewMediaMetadata) {
                    var playlistSelectedEventArgs = (PlaylistSelectedEventArgs)e;
                    var viewMediaMetadata = (ViewMediaMetadata)uie.DataContext;
                    EntitiesHelper.AddPlaylistMediaMetadata(viewMediaMetadata.UniqueId, playlistSelectedEventArgs.SelectedPlaylist.UniqueId);
                }
            }
        }

        private async void ExtraToolsSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = (ListBoxItem)e.AddedItems[0];
                var viewMediaMetadata = (ViewMediaMetadata)item.DataContext;
                switch (item.Content)
                {
                    case "Delete Media": 
                        await DeleteMedia(viewMediaMetadata.YID, viewMediaMetadata.MediaType);
                        OnMediaDeleted?.Invoke(null, null);
                        break;
                    case "Open Images Editor": OpenImagesEditor(viewMediaMetadata); break;
                    case "Pin to Start": break;
                    case "Open in YouTube": break;
                    case "Copy URL to Clipboard": break;
                }
            }
            if (sender is ListBox) {
                ((ListBox)sender).SelectedIndex = -1;
            }
            XamlHelper.CloseFlyout(sender);
        }

        private void OpenImagesEditor(object viewModel) {
            var windowContent = new ImagesEditor();
            windowContent.DataContext = viewModel;
            WindowHelper.OpenWindow(windowContent, WindowHelper.DefaultEditorWindowWidth, WindowHelper.DefaultEditorWindowHeight, ()=> { windowContent.InitialSetup(); });            
        }

        private async Task DeleteMedia(string yid, string fileType) {
            // use YId as the key for deleting
            var mediaPathFolder = await StorageFolder.GetFolderFromPathAsync(mediaPath);
            if (mediaPathFolder != null) {

                // get extra content folder if it exists & delete it
                await StorageHelper.TryDeleteFolder(yid, mediaPathFolder);
                
                // delete root images
                await StorageHelper.TryDeleteFile($"{yid}-high.jpg", mediaPathFolder);
                await StorageHelper.TryDeleteFile($"{yid}-medium.jpg", mediaPathFolder);
                await StorageHelper.TryDeleteFile($"{yid}-low.jpg", mediaPathFolder);
                await StorageHelper.TryDeleteFile($"{yid}-max.jpg", mediaPathFolder);
                await StorageHelper.TryDeleteFile($"{yid}-standard.jpg", mediaPathFolder);

                // delete root mp3/mp4
                await StorageHelper.TryDeleteFile($"{yid}.{fileType}", mediaPathFolder);
            }

            // delete DB data
            EntitiesHelper.DeleteAllByYID(yid);
        }

        private void ShowTile(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var grdContainer = (Grid)sender;
            if (grdContainer.Children.Count == 1 && grdContainer.DataContext is ViewMediaMetadata) {
                var viewMediaMetadata = (ViewMediaMetadata)grdContainer.DataContext;
                var newTile = new ImageEditorTile() { Width = grdContainer.Width, Height = grdContainer.Height, Direction = RotatorTile.RotateDirection.Left };
                grdContainer.Children.Add(newTile);
                newTile.InitialSetup(viewMediaMetadata);
            }
        }

        private void RemoveTile(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var grdContainer = (Grid)sender;
            if (grdContainer.Children.Count > 1)
            {
                var lastChild = grdContainer.Children[grdContainer.Children.Count - 1];
                grdContainer.Children.Remove(lastChild);
            }
            
        }
    }

    public class PlayMediaEventArgs : EventArgs {
        public ViewMediaMetadata ViewMediaMetadata { get; set; }
    }
}

// https://docs.microsoft.com/en-us/windows/uwp/design/layout/app-window