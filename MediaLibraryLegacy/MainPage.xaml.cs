using SharedCode.SQLite;
using System;
using Windows.UI.Xaml.Controls;

namespace MediaLibraryLegacy
{

    public sealed partial class MainPage : Page
    {
        private const string mediaPath = "d:\\deleteme\\downloadedMedia";
        private const string dbName = "youtubewatcher";
        private const string youtubeHomeUrl = "https://www.youtube.com";
        private const double taskbarHeight = 40;


        public MainPage()
        {
            this.InitializeComponent();

            // initialize the sqlite db
            AppDatabase.Current(mediaPath, dbName).Init();

            // setup views
            viewMediaLibrary.SetupLibraryView(mediaPath);
        }

        private void PlayMedia(object sender, PlayMediaEventArgs e)
        {
            viewMediaPlayer.OpenMediaUri(new Uri($"{mediaPath}\\{e.ViewMediaMetadata.YID}.mp4", UriKind.Absolute));
            viewMediaPlayer.ShowHideMediaPlayer(true, e.ViewMediaMetadata.Title);
        }
    }


    public struct ViewMediaMetadata
    {
        public string YID { get; set; }
        public string Title { get; set; }
        public Uri ThumbUri { get; set; }
        public string Quality { get; set; }
        public string MediaType { get; set; }
        public long Size { get; set; }
    }
}
