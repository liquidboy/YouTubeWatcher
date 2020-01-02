using Microsoft.Toolkit.Wpf.UI.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using YoutubeExplode;
using SharedCode.SQLite;
using SharedCode.YT;
using System.Windows.Data;
using System.Globalization;

namespace YouTubeWatcher
{
    public partial class MainWindow : Window
    {
        private const string mediaPath = "d:\\deleteme\\downloadedMedia";
        private const string dbName = "youtubewatcher";
        private const string youtubeHomeUrl = "https://www.youtube.com";
        private const double taskbarHeight = 40;
        
        private IYoutubeClientHelper clientHelper;
        private WebView wvMain;
        private Queue<MediaJob> jobQueue = new Queue<MediaJob>();
        
        public MainWindow()
        {
            InitializeComponent();

            // initialize the sqlite db
            AppDatabase.Current(mediaPath, dbName).Init();  

            // initialize Youtube helpers
            clientHelper = new YoutubeClientHelper(new YoutubeClient(), System.AppDomain.CurrentDomain.BaseDirectory);

            // setup views
            SetupWebView();
            SetupLibraryView();

            //TestSqliteBits();
            UpdateLibraryStatistics();
            UpdateJobStatistics();
        }

        private void SetupLibraryView() {
            ShowHideLibrary(false);
            tbMediaDirectory.Text = mediaPath;
        }

        private void SetupWebView() {
            var process = new Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlProcess();
            //process.ProcessExited += Process_ProcessExited;
            wvMain = new WebView(process);
            wvMain.Margin = new Thickness(0, 0, 0, taskbarHeight);
            wvMain.ContentLoading += WvMain_ContentLoading;
            layoutRoot.Children.Add(wvMain);
            wvMain.Source = new Uri(youtubeHomeUrl);
        }

        private void TestSqliteBits() {
            //create new
            var newEntity = new MediaMetadata()
            {
                YID = Guid.NewGuid().ToString(),
                Title = "test title",
                DateStamp = DateTime.UtcNow,
                ThumbUrl = string.Empty,
            };
            var newid = DBContext.Current.Save(newEntity);

            //search
            var foundData = DBContext.Current.RetrieveEntity<MediaMetadata>(newEntity.UniqueId);

            var foundItems = DBContext.Current.RetrieveEntities<MediaMetadata>($"Title=='test title'");

            var foundItems2 = DBContext.Current.RetrieveAllEntities<MediaMetadata>();

            ////load 
            //if (DBContext.Current.RetrieveEntity<MediaMetadata>(oh.UniqueId) != null)
            //{
            //    //delete
            //    DBContext.Current.DeleteEntity<MediaMetadata>(oh.UniqueId);
            //}

            ////delete
            //DBContext.Current.DeleteAll<MediaMetadata>();
            //var foundItems3 = DBContext.Current.RetrieveAllEntities<MediaMetadata>();


            ////delete from manager
            //DBContext.Current.Manager.DeleteAllDatabases();
        }

        private string lastProcessedUrl;

        private async void WvMain_ContentLoading(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlContentLoadingEventArgs e)
        {
            var url = await wvMain.InvokeScriptAsync("eval", new string[] { "document.location.href;" });
            if (HasUrlBeenProcessed(url)) return;

            lastProcessedUrl = url;
            tbUrl.Text = url;
            cbFormats.Items.Clear();
            spDownloadToolbar.Visibility = Visibility.Collapsed;

            if (IsValidUrl(url)) {
                var videoDetail = await GetVideoDetails(url);
                if (videoDetail != null)
                {
                    foreach (var mt in videoDetail.qualities)
                    {
                        cbFormats.Items.Add(new ComboBoxItem() { Content = mt });
                    }
                }
                ShouldWeShowToolbar();
                await DownloadMediumThumbnail(videoDetail);
            }
        }

        private bool HasUrlBeenProcessed(string urlToProcess) => urlToProcess.Equals(lastProcessedUrl, StringComparison.CurrentCultureIgnoreCase);

        private async Task<VideoDetails> GetVideoDetails(string ytUrl) {
            if (!IsValidUrl(ytUrl)) return null;
            try {
                return await clientHelper.GetVideoMetadata(clientHelper.GetVideoID(ytUrl));
            }
            catch (Exception ex) {
                // todo: handle error
            }
            return null;
        }

        private async Task DownloadThumbnails(VideoDetails videoDetails) {
            if (videoDetails == null) return;
            await DownloadImageAsync($"{videoDetails.id}-low", new Uri(videoDetails.thumbnails.LowResUrl));
            //await DownloadImageAsync($"{videoDetails.id}-medium", new Uri(videoDetails.thumbnails.MediumResUrl));
            await DownloadImageAsync($"{videoDetails.id}-standard", new Uri(videoDetails.thumbnails.StandardResUrl));
            await DownloadImageAsync($"{videoDetails.id}-high", new Uri(videoDetails.thumbnails.HighResUrl));
            await DownloadImageAsync($"{videoDetails.id}-max", new Uri(videoDetails.thumbnails.MaxResUrl));
        }

        private async Task DownloadMediumThumbnail(VideoDetails videoDetails)
        {
            if (videoDetails == null) return;
            await DownloadImageAsync($"{videoDetails.id}-medium", new Uri(videoDetails.thumbnails.MediumResUrl));
        }

        private async Task DownloadImageAsync(string fileName, Uri uri)
        {
            try
            {
                using var httpClient = new System.Net.Http.HttpClient();

                // Get the file extension
                var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                var fileExtension = System.IO.Path.GetExtension(uriWithoutQuery);

                // Download the image and write to the file
                var path = System.IO.Path.Combine(mediaPath, $"{fileName}{fileExtension}");
                var imageBytes = await httpClient.GetByteArrayAsync(uri);
                await File.WriteAllBytesAsync(path, imageBytes);
            }
            catch { }
        }

        private void ShouldWeShowToolbar() {
            if (IsValidUrl(tbUrl.Text)) spDownloadToolbar.Visibility = Visibility.Visible;
            else spDownloadToolbar.Visibility = Visibility.Collapsed;
        }

        private bool IsValidUrl(string ytUrl) {
            if (ytUrl == "https://www.youtube.com/") return false;
            if (string.IsNullOrEmpty(ytUrl)) return false;
            return true;
        }

        private async void butLoad_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValidUrl(tbUrl.Text)) return;

            var mediaType = (string)((ComboBoxItem)cbMediaType.SelectedValue).Content;
            var quality = (mediaType != "mp3") ? (string)((ComboBoxItem)cbFormats.SelectedValue).Content : string.Empty;
            var mediaJob = new MediaJob() { YoutubeUrl = tbUrl.Text, MediaType = mediaType, Quality = quality };

            jobQueue.Enqueue(mediaJob);

            ProcessJobFromQueue();
        }

        private bool isProcessingJob = false;
        private async void ProcessJobFromQueue() 
        {
            UpdateStatus();
            UpdateLibraryStatistics();
            UpdateJobStatistics();
            if (isProcessingJob) return;
            UpdateStatusImage(null);
            if (jobQueue.Count == 0) return; 
            var job = jobQueue.Dequeue();
            isProcessingJob = true;
            UpdateJobStatistics();
            var videoDetails = await GetVideoDetails(job.YoutubeUrl);
            UpdateStatusImage(videoDetails);
            await ProcessYoutubeVideo(videoDetails, job.MediaType, job.Quality);
            isProcessingJob = false;
        }

        private async Task ProcessYoutubeVideo(VideoDetails videoDetails, string mediaType, string quality) 
        {
            if (videoDetails == null) return;

            await DownloadThumbnails(videoDetails);

            var mediaPath = MainWindow.mediaPath + $"\\{videoDetails.id}.{mediaType}";

            try
            {
                if (File.Exists(mediaPath)) File.Delete(mediaPath);
                await clientHelper.DownloadMedia(videoDetails.id, quality, mediaPath, mediaType);
                var fileInfo = new FileInfo(mediaPath);
                RecordMetadata(videoDetails, mediaType, quality, fileInfo.Length);
            }
            catch (Exception ex)
            {
                // todo: handle error
            }

            UpdateLibraryStatistics();
            UpdateJobStatistics();
            isProcessingJob = false;
            ProcessJobFromQueue();
        }

        private void UpdateStatus()
        {
            tbStatus.Text = (jobQueue.Count > 0) ? "downloading media" : string.Empty;
            tbStatusTitle.Text = (jobQueue.Count > 0) ? "job" : string.Empty;
            spStatus.Visibility = (jobQueue.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateStatusImage(VideoDetails videoDetails)
        {
            if (videoDetails == null) {
                imgStatus.Source = null;
                return;
            }

            var uri = new Uri($"{mediaPath}\\{videoDetails.id}-medium.jpg", UriKind.Absolute);
            imgStatus.Source = new System.Windows.Media.Imaging.BitmapImage(uri);
        }

        private void RecordMetadata(VideoDetails videoDetails, string mediaType, string quality, long size)
        {
            var newEntity = new MediaMetadata()
            {
                YID = videoDetails.id,
                Title = videoDetails.Title,
                DateStamp = DateTime.UtcNow,
                ThumbUrl = videoDetails.thumbnails.MediumResUrl,
                MediaType = mediaType,
                Quality = quality,
                Size = size
            };

            var newid = DBContext.Current.Save(newEntity);
        }

        private void UpdateLibraryStatistics() {
            var foundItems = DBContext.Current.RetrieveAllEntities<MediaMetadata>();
            var libraryCount = (foundItems == null) ? 0 : foundItems.Count ;
            tbLibraryCount.Text = libraryCount.ToString();
        }

        private void UpdateJobStatistics() => tbJobsCount.Text = jobQueue.Count.ToString();
        private void ShowLibrary(object sender, RoutedEventArgs e) => ShowHideLibrary(true);
        private void butCloseLibrary_Click(object sender, RoutedEventArgs e) => ShowHideLibrary(false);
        private void butShowMediaFolder_Click(object sender, RoutedEventArgs e) => OpenMediaFolder();

        private void ShowHideLibrary(bool show)
        {
            wvMain.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
            grdLibrary.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            LoadLibraryItems(show);
            ShowHideMediaPlayer(false);
        }

        private void LoadLibraryItems(bool load)
        {
            if (load)
            {
                var MediaItems = new ObservableCollection<ViewMediaMetadata>();
                var foundItems = DBContext.Current.RetrieveAllEntities<MediaMetadata>();
                foundItems.Reverse();
                foreach (var foundItem in foundItems)
                {
                    MediaItems.Add(new ViewMediaMetadata()
                    {
                        Title = foundItem.Title,
                        YID = foundItem.YID,
                        ThumbUri = new Uri($"{mediaPath}\\{foundItem.YID}-medium.jpg", UriKind.Absolute),
                        Quality = foundItem.Quality,
                        MediaType = foundItem.MediaType,
                        Size = foundItem.Size,
                    });

                }
                icLibraryItems.ItemsSource = MediaItems;
            }
            else
            {
                //icLibraryItems.Items.Clear();
                icLibraryItems.ItemsSource = null;
            }
        }

        private void OpenMediaFolder() {
            Process process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = mediaPath;
            process.Start();
        }

        private void PlayMedia(object sender, RoutedEventArgs e)
        {
            var but = sender as Button;
            if (but.DataContext is ViewMediaMetadata) {
                var vmd = (ViewMediaMetadata)but.DataContext;

                mePlayer.Source = new Uri($"{mediaPath}\\{vmd.YID}.mp4", UriKind.Absolute);
                ShowHideMediaPlayer(true);
            }
        }

        private void ShowHideMediaPlayer(bool show) {
            if (show)
            {
                grdMediaPlayer.Visibility = Visibility.Visible;
                isPlaying = true;
                mePlayer.Play();
            }
            else {
                mePlayer.Stop();
                isPlaying = false;
                mePlayer.Source = null;
                mePlayerSlider.Value = 0;
                grdMediaPlayer.Visibility = Visibility.Collapsed;
            }
        }

        bool isPlaying = false;
        private void TogglePausePlay() {
            if (isPlaying) mePlayer.Pause();
            else mePlayer.Play();
            isPlaying = !isPlaying;
        }

        private void grdMediaPlayer_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e) => TogglePausePlay();

        private void CloseMediaPlayer(object sender, RoutedEventArgs e) => ShowHideMediaPlayer(false);

        private void ScrubMedia(object sender, RoutedPropertyChangedEventArgs<double> e) => mePlayer.Position = new TimeSpan(0, 0, 0, (int)mePlayerSlider.Value, 0);

        private void mePlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            mePlayerSlider.Minimum = 0;
            mePlayerSlider.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
        }

        
    }

    public struct MediaJob{
        public VideoDetails VideoDetails;
        public string YoutubeUrl;
        public string MediaType;
        public string Quality;
    }

    public struct ViewMediaMetadata {
        public string YID { get; set;  }
        public string Title { get; set; }
        public Uri ThumbUri { get; set; }
        public string Quality { get; set; }
        public string MediaType { get; set; }
        public long Size { get; set; }
    }


    [ValueConversion(typeof(long), typeof(string))]
    public class FileSizeToStringConverter : IValueConverter
    {
        public static FileSizeToStringConverter Instance { get; } = new FileSizeToStringConverter();

        private static readonly string[] Units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return default(string);

            double size = (long)value;
            var unit = 0;

            while (size >= 1024)
            {
                size /= 1024;
                ++unit;
            }

            return $"{size:0.#} {Units[unit]}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
