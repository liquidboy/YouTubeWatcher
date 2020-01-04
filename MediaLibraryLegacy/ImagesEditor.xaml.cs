using System;
using Windows.Media.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MediaLibraryLegacy
{
    public sealed partial class ImagesEditor : UserControl
    {
        public ImagesEditor()
        {
            this.InitializeComponent();
        }

        private void InitialSetup() {
            if (this.DataContext is ViewMediaMetadata) {
                var viewMediaMetadata = (ViewMediaMetadata)this.DataContext;
                var mediaUri = new Uri($"{App.mediaPath}\\{viewMediaMetadata.YID}.{viewMediaMetadata.MediaType}", UriKind.Absolute);
                mePlayer.Source = MediaSource.CreateFromUri(mediaUri);
            }
        }

        private void layoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            InitialSetup();
        }
    }
}
