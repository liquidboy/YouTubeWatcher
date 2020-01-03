using SharedCode.SQLite;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MediaLibraryLegacy
{
    public sealed partial class PlayList : UserControl
    {
        string mediaPath;

        public event EventHandler OnCloseLibrary;

        public PlayList()
        {
            this.InitializeComponent();
        }

        public void InitialSetup(string mediapath)
        {
            mediaPath = mediapath;
        }

        public void Hide() {
            grdPlaylist.Visibility = Visibility.Collapsed;
            gvPlaylists.ItemsSource = null;
            icMediaItems.ItemsSource = null;
        }

        public void Show() { 
            grdPlaylist.Visibility = Visibility.Visible;
            LoadPlaylistItems();
        }

        private void CloseLibrary(object sender, RoutedEventArgs e) => OnCloseLibrary.Invoke(null, null);

        private void OnPlaylistCreated(object sender, EventArgs e)
        {
            butAddPlaylist.Flyout.Hide();
            LoadPlaylistItems();
        }

        private void LoadPlaylistItems()
        {
            var items = new ObservableCollection<ViewPlaylistMetadata>();
            var foundItems = DBContext.Current.RetrieveAllEntities<PlaylistMetadata>();
            foundItems.Reverse();
            foreach (var foundItem in foundItems)
            {
                items.Add(new ViewPlaylistMetadata()
                {
                    UniqueId = foundItem.UniqueId,
                    Title = foundItem.Title
                });

            }
            gvPlaylists.ItemsSource = items;

        }

        private void LoadPlaylist(Guid playlistUid)
        {
            var items = new ObservableCollection<ViewMediaMetadata>();
            var foundItems = DBContext.Current.RetrieveEntities<PlaylistMediaMetadata>($"PlaylistUid='{playlistUid.ToString()}'");
            var sqlIn = string.Empty;
            foreach (var foundItem in foundItems)
            {
                sqlIn += $"'{foundItem.MediaUid}' ,";
            }

            if (sqlIn.Length > 0) {
                sqlIn = sqlIn.Substring(0, sqlIn.Length - 1);
                var foundItems2 = DBContext.Current.RetrieveEntities<MediaMetadata>($"UniqueId IN ({sqlIn})");
                foreach (var foundItem in foundItems2)
                {
                    items.Add(new ViewMediaMetadata()
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
            }

            icMediaItems.ItemsSource = items;
        }

        private void PlaylistChanged(object sender, SelectionChangedEventArgs e)
        {
            icMediaItems.ItemsSource = null;

            if (e.AddedItems.Count > 0) {
                foreach (var item in e.AddedItems) {
                    if(item is ViewPlaylistMetadata)LoadPlaylist(((ViewPlaylistMetadata)item).UniqueId);    
                }
            }   
        }
    }
}
