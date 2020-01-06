using Microsoft.Toolkit.Uwp.UI.Controls;
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

namespace MediaLibraryLegacy.Controls
{
    public sealed partial class TileOnHover : UserControl
    {
        public TileOnHover()
        {
            this.InitializeComponent();
        }

        private void ShowTile(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var grdContainer = (Grid)sender;
            if (grdContainer.Children.Count == 1 && grdContainer.DataContext is ViewMediaMetadata)
            {
                var viewMediaMetadata = (ViewMediaMetadata)grdContainer.DataContext;
                var newTile = new SnapshotsTile() { Width = grdContainer.Width, Height = grdContainer.Height, Direction = RotatorTile.RotateDirection.Left };
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
}
