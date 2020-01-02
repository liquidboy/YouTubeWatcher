using System;
using Windows.Media.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace MediaLibraryLegacy
{
    public sealed partial class MediaPlayer : UserControl
    {
        public MediaPlayer()
        {
            this.InitializeComponent();
        }

        public void ShowHideMediaPlayer(bool show, string title = "")
        {
            if (show)
            {
                grdMediaPlayer.Visibility = Visibility.Visible;
                tbMediaPlayerTitle.Text = title;
                isPlaying = true;
                mePlayer.MediaPlayer.Play();
            }
            else
            {
                mePlayer.MediaPlayer.Pause();
                tbMediaPlayerTitle.Text = string.Empty;
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

        public void OpenMediaUri(Uri mediaUri, Uri thumbUri, Uri wallpaperUri) {
            mePlayer.Source = MediaSource.CreateFromUri(mediaUri);
            imgThumb.Source = new BitmapImage(thumbUri);
            imgWallpaper.Source = new BitmapImage(wallpaperUri);
        }

        private void CloseMediaPlayer(object sender, RoutedEventArgs e) => ShowHideMediaPlayer(false);
    }
}
