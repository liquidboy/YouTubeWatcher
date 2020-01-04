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
                var MediaItems = new ObservableCollection<ViewMediaMetadata>();
                var foundItems = DBContext.Current.RetrieveAllEntities<MediaMetadata>();
                foundItems.Reverse();
                foreach (var foundItem in foundItems)
                {
                    MediaItems.Add(new ViewMediaMetadata()
                    {
                        UniqueId = foundItem.UniqueId,
                        Title = foundItem.Title,
                        YID = foundItem.YID,
                        ThumbUri = new Uri($"{mediaPath}\\{foundItem.YID}-medium.jpg", UriKind.Absolute),
                        Quality = foundItem.Quality,
                        MediaType = foundItem.MediaType,
                        Size = foundItem.Size,
                    });
                }
                icLibraryItems.ItemsSource = MediaItems;
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
                    RecordMetadata(viewMediaMetadata.UniqueId, playlistSelectedEventArgs.SelectedPlaylist.UniqueId);
                }
            }
        }

        private void RecordMetadata(Guid mediaUid, Guid playlistUid)
        {
            var foundEntities = DBContext.Current.RetrieveEntities<PlaylistMediaMetadata>($"MediaUid='{mediaUid.ToString()}' and PlaylistUid='{playlistUid.ToString()}'");

            if (foundEntities.Count == 0) {
                var newEntity = new PlaylistMediaMetadata()
                {
                    MediaUid = mediaUid,
                    PlaylistUid = playlistUid,
                    DateStamp = DateTime.UtcNow
                };
                DBContext.Current.Save(newEntity);
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
            WindowHelper.OpenWindow(windowContent, WindowHelper.DefaultEditorWindowWidth, WindowHelper.DefaultEditorWindowHeight);            
        }

        private async Task DeleteMedia(string yid, string fileType) {
            // use YId as the key for deleting
            var mediaPathFolder = await StorageFolder.GetFolderFromPathAsync(mediaPath);
            if (mediaPathFolder != null) {

                // get extra content folder if it exists & delete it
                await FileFolderHelper.TryDeleteFolder(yid, mediaPathFolder);
                
                // delete root images
                await FileFolderHelper.TryDeleteFile($"{yid}-high.jpg", mediaPathFolder);
                await FileFolderHelper.TryDeleteFile($"{yid}-medium.jpg", mediaPathFolder);
                await FileFolderHelper.TryDeleteFile($"{yid}-low.jpg", mediaPathFolder);
                await FileFolderHelper.TryDeleteFile($"{yid}-max.jpg", mediaPathFolder);
                await FileFolderHelper.TryDeleteFile($"{yid}-standard.jpg", mediaPathFolder);

                // delete root mp3/mp4
                await FileFolderHelper.TryDeleteFile($"{yid}.{fileType}", mediaPathFolder);
            }
           
            // delete DB data
            var foundMediaMetadata = DBContext.Current.RetrieveEntities<MediaMetadata>($"YID='{yid}'");
            if (foundMediaMetadata.Count > 0)
            {
                var uniqueId = foundMediaMetadata[0].UniqueId;

                // - delete from MediaMetadata
                DBContext.Current.DeleteEntity<MediaMetadata>(uniqueId);

                // - delete from PlaylistMediaMetadata
                var foundPlaylistMediaMetadata = DBContext.Current.RetrieveEntities<PlaylistMediaMetadata>($"MediaUid='{uniqueId.ToString()}'");

                if (foundPlaylistMediaMetadata.Count > 0)
                {
                    foreach (var entity in foundPlaylistMediaMetadata) {
                        DBContext.Current.DeleteEntity<PlaylistMediaMetadata>(entity.UniqueId);
                    }
                }
            }
        }
    }

    public class PlayMediaEventArgs : EventArgs {
        public ViewMediaMetadata ViewMediaMetadata { get; set; }
    }
}

// https://docs.microsoft.com/en-us/windows/uwp/design/layout/app-window