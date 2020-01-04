using SharedCode.SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MediaLibraryLegacy
{
    public sealed partial class PlaylistChooser : UserControl
    {
        public event EventHandler OnItemSelected;

        public PlaylistChooser()
        {
            this.InitializeComponent();

            LoadPlaylistItems();
        }

        private void LoadPlaylistItems() => gvPlaylists.ItemsSource = EntitiesHelper.RetrievePlaylistMetadataAsViewCollection();

        private void PlaylistSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0) {
                var flyout = (Windows.UI.Xaml.Controls.FlyoutPresenter)this.Parent;
                OnItemSelected.Invoke(flyout, new PlaylistSelectedEventArgs() { SelectedPlaylist = (ViewPlaylistMetadata)e.AddedItems[0] });
            }
        }
    }

    public class PlaylistSelectedEventArgs : EventArgs {
        public ViewPlaylistMetadata SelectedPlaylist;
    }
}
