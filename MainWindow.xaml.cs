using Microsoft.Toolkit.Wpf.UI.Controls;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using YouTubeWatcher.SQLite;
using YouTubeWatcher.YT;

namespace YouTubeWatcher
{
    public partial class MainWindow : Window
    {
        private IYoutubeClientHelper clientHelper;
        private const string workingPath = "d:\\deleteme\\downloadedMedia";
        private const string dbName = "youtubewatcher";
        private string installLocation = System.AppDomain.CurrentDomain.BaseDirectory;
        private WebView wvMain;
        private Queue<MediaJob> jobQueue = new Queue<MediaJob>();
        
        public MainWindow()
        {
            InitializeComponent();

            AppDatabase.Current(workingPath, dbName).Init();  // initialize the sqlite db

            clientHelper = new YoutubeClientHelper(new YoutubeClient(), installLocation);

            Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlProcess process = new Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlProcess();
            //process.ProcessExited += Process_ProcessExited;
            wvMain = new WebView(process);
            wvMain.Margin = new Thickness(0, 0, 0, 30);
            wvMain.ContentLoading += WvMain_ContentLoading;
            layoutRoot.Children.Add(wvMain);
            wvMain.Source = new Uri("https://www.youtube.com");

            //TestSqliteBits();
            UpdateStatistics();
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

        private async void WvMain_ContentLoading(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlContentLoadingEventArgs e)
        {
            var url = await wvMain.InvokeScriptAsync("eval", new String[] { "document.location.href;" });
            tbUrl.Text = url;

            var videoDetail = await GetVideoDetails(url);
            if (videoDetail != null)
            {
                VideoChanging();
                foreach (var mt in videoDetail.qualities)
                {
                    cbFormats.Items.Add(new ComboBoxItem() { Content = mt });
                }
                VideoChanged();
            }
        }

        private void VideoChanging() {
            cbFormats.Items.Clear();
            butLoad.IsEnabled = false;
        }

        private void VideoChanged() {
            butLoad.IsEnabled = true;
        }

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
            await DownloadImageAsync($"{videoDetails.id}-medium", new Uri(videoDetails.thumbnails.MediumResUrl));
            await DownloadImageAsync($"{videoDetails.id}-standard", new Uri(videoDetails.thumbnails.StandardResUrl));
            await DownloadImageAsync($"{videoDetails.id}-high", new Uri(videoDetails.thumbnails.HighResUrl));
            await DownloadImageAsync($"{videoDetails.id}-max", new Uri(videoDetails.thumbnails.MaxResUrl));
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
                var path = System.IO.Path.Combine(workingPath, $"{fileName}{fileExtension}");
                var imageBytes = await httpClient.GetByteArrayAsync(uri);
                await File.WriteAllBytesAsync(path, imageBytes);
            }
            catch { }
        }

        private bool IsValidUrl(string ytUrl) {
            if (ytUrl == "https://www.youtube.com/") return false;
            if (string.IsNullOrEmpty(ytUrl)) return false;
            return true;
        }

        private async void butLoad_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValidUrl(tbUrl.Text)) return;
            jobQueue.Enqueue(new MediaJob() { YoutubeUrl = tbUrl.Text });
            ProcessJobFromQueue();
        }

        private bool isProcessingJob = false;
        private async void ProcessJobFromQueue() 
        {
            UpdateStatus();
            UpdateStatistics();
            if (isProcessingJob) return;
            if (jobQueue.Count == 0) return;
            var job = jobQueue.Dequeue();
            isProcessingJob = true;
            UpdateStatistics();
            var videoDetails = await GetVideoDetails(job.YoutubeUrl);
            await ProcessYoutubeVideo(videoDetails);
            isProcessingJob = false;
        }

        private async Task ProcessYoutubeVideo(VideoDetails videoDetails) 
        {
            if (videoDetails == null) return;

            await DownloadThumbnails(videoDetails);

            var mediaType = (string)((ComboBoxItem)cbMediaType.SelectedValue).Content;
            var quality = (mediaType != "mp3") ? (string)((ComboBoxItem)cbFormats.SelectedValue).Content : string.Empty;
            var mediaPath = workingPath + $"\\{videoDetails.id}.{mediaType}";

            try
            {
                if (File.Exists(mediaPath)) File.Delete(mediaPath);
                await clientHelper.DownloadMedia(videoDetails.id, quality, mediaPath, mediaType);
                RecordMetadata(videoDetails);
            }
            catch (Exception ex)
            {
                // todo: handle error
            }

            UpdateStatistics();
            isProcessingJob = false;
            ProcessJobFromQueue();
        }

        private void UpdateStatus() {

            if (jobQueue.Count > 0)
            {
                tbStatus.Text = " .. please wait downloading media .. ";
            }
            else
            {
                tbStatus.Text = string.Empty;
            }
        }

        private void RecordMetadata(VideoDetails videoDetails)
        {
            var newEntity = new MediaMetadata()
            {
                YID = videoDetails.id,
                Title = videoDetails.Title,
                DateStamp = DateTime.UtcNow,
                ThumbUrl = videoDetails.thumbnails.MediumResUrl,
            };
            var newid = DBContext.Current.Save(newEntity);
        }

        private void UpdateStatistics() {
            var foundItems = DBContext.Current.RetrieveAllEntities<MediaMetadata>();
            var libraryCount = (foundItems == null) ? 0 : foundItems.Count ;
            tbFeedback.Text = $"library : {libraryCount}  jobs : {jobQueue.Count}";
        }

    }

    public struct MediaJob{
        public VideoDetails VideoDetails;
        public string YoutubeUrl;
    }

}
