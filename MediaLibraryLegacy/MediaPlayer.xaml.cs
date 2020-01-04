using MediaLibraryLegacy.Controls;
using System;
using Windows.Media.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace MediaLibraryLegacy
{
    public sealed partial class MediaPlayer : UserControl
    {
        TextblockMarquee marquee;

        int[,] supportedDimensions = { { 600,  400}, { 300, 200 } , { 900, 600 } };
        int currentDimension = 0;


        public MediaPlayer()
        {
            this.InitializeComponent();
        }

        public void ShowHideMediaPlayer(bool show, string title = "")
        {
            if (show)
            {
                grdMediaPlayer.Visibility = Visibility.Visible;
                ChangeMarquee(title);
                isPlaying = true;
                mePlayer.MediaPlayer.Play();
            }
            else
            {
                mePlayer.MediaPlayer.Pause();
                ChangeMarquee(string.Empty);
                isPlaying = false;
                mePlayer.Source = null;
                imgThumb.Source = null;
                imgWallpaper.Source = null;
                grdMediaPlayer.Visibility = Visibility.Collapsed;
            }
        }

        bool isPlaying = false;
        private void TogglePausePlay()
        {
            if (isPlaying) mePlayer.MediaPlayer.Pause();
            else mePlayer.MediaPlayer.Play();
            isPlaying = !isPlaying;
        }

        private void ChangeMarquee(string title) {

            if (marquee != null)
            {
                marquee.SetText(string.Empty);
                grdHeader.Children.Remove(marquee);
                marquee = null;
            }

            if (!string.IsNullOrEmpty(title)) {
                spHeaderTools.UpdateLayout();
                marquee = new TextblockMarquee() { Margin = new Thickness(imgThumb.Width + 5, 3, spHeaderTools.ActualWidth + 5, 5) };
                grdHeader.Children.Add(marquee);
                marquee.SetText(title);
            }
        }

        public void OpenMediaUri(Uri mediaUri, Uri thumbUri, Uri wallpaperUri) {
            mePlayer.Source = MediaSource.CreateFromUri(mediaUri);
            imgThumb.Source = new BitmapImage(thumbUri);
            imgWallpaper.Source = new BitmapImage(wallpaperUri);
        }

        private void CloseMediaPlayer(object sender, RoutedEventArgs e) => ShowHideMediaPlayer(false);

        private void ResizeMediaPlayer(object sender, RoutedEventArgs e)
        {
            currentDimension++;
            if (currentDimension >= (supportedDimensions.Length / 2)) currentDimension = 0;
            grdMediaPlayer.Width = supportedDimensions[currentDimension, 0];
            grdMediaPlayer.Height = supportedDimensions[currentDimension, 1];

            ChangeMarquee(marquee.GetText());
        }
    }
}
