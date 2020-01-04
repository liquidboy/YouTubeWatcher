using SharedCode.SQLite;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MediaLibraryLegacy
{
    public sealed partial class PlaylistForm : UserControl
    {
        public event EventHandler OnPlaylistCreated;

        public PlaylistForm()
        {
            this.InitializeComponent();
        }

        private void CreatePlaylist(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbTitle.Text)) {
                EntitiesHelper.AddPlaylistMetadata(tbTitle.Text);
                tbTitle.Text = string.Empty;
            }
            OnPlaylistCreated?.Invoke(null, null);
        }
    }
}

// Flyout info
// https://docs.microsoft.com/en-us/previous-versions/windows/apps/dn308515(v=win.10)