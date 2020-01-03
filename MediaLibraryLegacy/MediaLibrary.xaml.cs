using SharedCode.SQLite;
using System;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MediaLibraryLegacy
{
    public sealed partial class MediaLibrary : UserControl
    {
        string mediaPath;

        public event EventHandler<PlayMediaEventArgs> OnPlayMedia;
        public event EventHandler OnCloseLibrary;

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

        private void CloseLibrary(object sender, RoutedEventArgs e) => OnCloseLibrary.Invoke(null, null);
    }

    public class PlayMediaEventArgs : EventArgs {
        public ViewMediaMetadata ViewMediaMetadata { get; set; }
    }
}
