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
            viewYoutubePlayer.InitialSetup();
            viewTaskbar.InitialSetup(App.mediaPath);
            //viewYoutubePlayer.LoadUri(new Uri(youtubeHomeUrl));
            SetupInitialView();
        }

        private void SetupInitialView() {
            viewMediaLibrary.ShowHideLibrary(false);
            viewYoutubePlayer.LoadUri(new Uri(App.youtubeHomeUrl));
        }

        private void PlayMedia(object sender, PlayMediaEventArgs e)
        {
            viewMediaPlayer.OpenMediaUri(new Uri($"{App.mediaPath}\\{e.ViewMediaMetadata.YID}.mp4", UriKind.Absolute));
            viewMediaPlayer.ShowHideMediaPlayer(true, e.ViewMediaMetadata.Title);
        }

        private void MediaChanged(object sender, MediaChangedEventArgs e)
        {
            viewTaskbar.MediaChanged(e.MediaUri);
        }

        private void OnShowLibrary(object sender, EventArgs e)
        {
            viewYoutubePlayer.Hide();
            viewMediaLibrary.ShowHideLibrary(true);
        }

        private void OnCloseLibrary(object sender, EventArgs e)
        {
            viewYoutubePlayer.Show();
            viewMediaLibrary.ShowHideLibrary(false);
        }
    }
}