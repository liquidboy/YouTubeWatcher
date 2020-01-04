using SharedCode.SQLite;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MediaLibraryLegacy
{
    public sealed partial class PlayList : UserControl
    {
        string mediaPath;
        Guid lastSelectedPlaylistId = Guid.Empty;

        public event EventHandler OnPlaylistAdded;
        public event EventHandler<PlayMediaEventArgs> OnPlayMedia;

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
            SetupInitialView();
        }

        private void SetupInitialView() {
            if (lastSelectedPlaylistId != Guid.Empty) {
                var result = EntitiesHelper.RetrievePlaylistMediaMetadataAsViewCollection(lastSelectedPlaylistId, mediaPath);
                icMediaItems.ItemsSource = result.source;
                lastSelectedPlaylistId = result.lastSelectedPlaylistId;
            }
        }

        private void OnPlaylistCreated(object sender, EventArgs e)
        {
            butAddPlaylist.Flyout.Hide();
            LoadPlaylistItems();
            OnPlaylistAdded?.Invoke(null, null);
        }

        
        private void LoadPlaylistItems()
        {
            var result = EntitiesHelper.RetrievePlaylistMetadataAsViewCollection(lastSelectedPlaylistId);
            gvPlaylists.ItemsSource = result.source;
            lastSelectedPlaylistId = result.lastSelectedPlaylistId;

        }

        private void PlaylistChanged(object sender, SelectionChangedEventArgs e)
        {
            icMediaItems.ItemsSource = null;

            if (e.AddedItems.Count > 0) {
                foreach (var item in e.AddedItems) {
                    if (item is ViewPlaylistMetadata)
                    {
                        var result = EntitiesHelper.RetrievePlaylistMediaMetadataAsViewCollection(((ViewPlaylistMetadata)item).UniqueId, mediaPath);
                        icMediaItems.ItemsSource = result.source;
                        lastSelectedPlaylistId = result.lastSelectedPlaylistId;
                    }
                }
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
    }
}
