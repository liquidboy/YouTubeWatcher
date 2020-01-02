using System;
using System.Collections.Generic;
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
