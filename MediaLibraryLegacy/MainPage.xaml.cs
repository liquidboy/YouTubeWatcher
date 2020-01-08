using SharedCode.SQLite;
using System;
using Windows.UI.Xaml.Controls;

namespace MediaLibraryLegacy
{
    public sealed partial class MainPage : Page
    {
        private const double taskbarHeight = 40;

        public MainPage()
        {
            this.InitializeComponent();

            // initialize the sqlite db
            AppDatabase.Current(App.mediaPath, App.dbName).Init();

            // setup views
            viewMediaLibrary.InitialSetup(App.mediaPath);
            viewMediaLibrary.InitialSetup(App.mediaPath);
            viewYoutubePlayer.InitialSetup();
            viewTaskbar.InitialSetup(App.mediaPath);
            viewPlaylist.InitialSetup(App.mediaPath);

            SetupInitialView();
        }

        private void SetupInitialView() {
            viewMediaLibrary.Hide();
            viewPlaylist.Hide();
            viewYoutubePlayer.LoadUri(new Uri(App.youtubeHomeUrl));
        }

        private void OnPlayMedia(object sender, PlayMediaEventArgs e)
        {
            var thumbUri = new Uri($"{App.mediaPath}\\{e.ViewMediaMetadata.YID}-medium.jpg", UriKind.Absolute);
            var mediaUri = new Uri($"{App.mediaPath}\\{e.ViewMediaMetadata.YID}.{e.ViewMediaMetadata.MediaType}", UriKind.Absolute);
            var wallpaperUri = new Uri($"{App.mediaPath}\\{e.ViewMediaMetadata.YID}-high.jpg", UriKind.Absolute);
            viewMediaPlayer.OpenMediaUri(mediaUri, thumbUri, wallpaperUri);
            viewMediaPlayer.ShowHideMediaPlayer(true, e.ViewMediaMetadata.Title);
        }

        private void OnMediaChanged(object sender, MediaChangedEventArgs e) => viewTaskbar.MediaChanged(e.MediaUri);

        private void OnShowLibrary(object sender, EventArgs e)
        {
            HideAllViews();
            viewMediaLibrary.Show();
        }

        private void OnShowPlaylist(object sender, EventArgs e)
        {
            HideAllViews();
            viewPlaylist.Show();
        }

        private void HideAllViews() {
            viewYoutubePlayer.Hide();
            viewMediaLibrary.Hide();
            viewPlaylist.Hide();
        }

        private void OnPlaylistAdded(object sender, EventArgs e)
        {
            viewTaskbar.UpdateStatistics();
            SendSystemNotification("Your new playlist was created!");
        }

        private void OnShowYoutube(object sender, EventArgs e)
        {
            HideAllViews();
            viewYoutubePlayer.Show();
        }

        private void OnMediaLibraryMediaDeleted(object sender, EventArgs e)
        {
            HideAllViews();
            viewMediaLibrary.Show();
            viewTaskbar.UpdateStatistics();
            SendSystemNotification("Media has been removed!");
        }

        private void SendSystemNotification(string message, int duration = 2000) => systemNotifications.Show(message, duration);

        private void OnOpenUrl(object sender, LaunchUrlEventArgs e) => viewYoutubePlayer.LoadUri(new Uri(e.Url));

        private void OnMediaAddedToPlaylist(object sender, EventArgs e) => SendSystemNotification("Media has been added to your playlist!");

        private void OnUrlCopiedToClipboard(object sender, EventArgs e) => SendSystemNotification("Media Url has been copied to your clipboard");
    }
}