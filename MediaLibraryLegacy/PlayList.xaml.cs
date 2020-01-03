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
                    Title = foundItem.Title
                });

            }
            gvPlaylists.ItemsSource = items;

        }
    }
}
