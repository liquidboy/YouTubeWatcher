using System;
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

        public void Hide() => grdPlaylist.Visibility = Visibility.Collapsed;

        public void Show() => grdPlaylist.Visibility = Visibility.Visible;

        private void CloseLibrary(object sender, RoutedEventArgs e) => OnCloseLibrary.Invoke(null, null);
    }
}
