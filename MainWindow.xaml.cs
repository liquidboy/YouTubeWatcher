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
        private string workingPath = "d:\\deleteme\\downloadedMedia";
        private string installLocation = System.AppDomain.CurrentDomain.BaseDirectory;
        private WebView wvMain;
        private VideoDetails selectedVideoDetail;

        public MainWindow()
        {
            InitializeComponent();
            
            AppDatabase.Current(workingPath).Init();

            clientHelper = new YoutubeClientHelper(new YoutubeClient(), installLocation);

            Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlProcess process = new Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlProcess();
            //process.ProcessExited += Process_ProcessExited;
            wvMain = new WebView(process);
            wvMain.Margin = new Thickness(0, 0, 0, 30);
            wvMain.ContentLoading += WvMain_ContentLoading;
            layoutRoot.Children.Add(wvMain);
            wvMain.Source = new Uri("https://www.youtube.com");

            TestSqliteBits();
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
            VideoChanging();
            await GetVideoDetails();
        }

        private void VideoChanging() {
            cbFormats.Items.Clear();
            butLoad.IsEnabled = false;
        }

        private void VideoChanged() {
            butLoad.IsEnabled = true;
        }

        private async Task GetVideoDetails() {
            if (!IsValidUrl()) return;
            selectedVideoDetail = null;
            try {
                selectedVideoDetail = await clientHelper.GetVideoMetadata(clientHelper.GetVideoID(tbUrl.Text));
                if (selectedVideoDetail != null)
                {
                    foreach (var mt in selectedVideoDetail.qualities)
                    {
                        cbFormats.Items.Add(new ComboBoxItem() { Content = mt });
                    }
                }
                VideoChanged();
            }
            catch (Exception ex) {
                // todo: handle error
            }
        }

        bool isDownloadingThumb = false;
        private async Task DownloadThumbnails() {
            if (selectedVideoDetail == null) return;
            if (isDownloadingThumb) return;
            isDownloadingThumb = true;
            await DownloadImageAsync($"{selectedVideoDetail.id}-low" , new Uri(selectedVideoDetail.thumbnails.LowResUrl));
            await DownloadImageAsync($"{selectedVideoDetail.id}-medium", new Uri(selectedVideoDetail.thumbnails.MediumResUrl));
            await DownloadImageAsync($"{selectedVideoDetail.id}-standard", new Uri(selectedVideoDetail.thumbnails.StandardResUrl));
            await DownloadImageAsync($"{selectedVideoDetail.id}-high", new Uri(selectedVideoDetail.thumbnails.HighResUrl));
            await DownloadImageAsync($"{selectedVideoDetail.id}-max", new Uri(selectedVideoDetail.thumbnails.MaxResUrl));
            isDownloadingThumb = false;
        }

        private async Task DownloadImageAsync(string fileName, Uri uri)
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

        private bool IsValidUrl() {
            if (tbUrl.Text == "https://www.youtube.com/") return false;
            if (string.IsNullOrEmpty(tbUrl.Text)) return false;
            return true;
        }

        bool isDownloadingVideo = false;
        private async void butLoad_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValidUrl()) return;
            if (selectedVideoDetail == null) return;
            if (isDownloadingVideo) return;

            await DownloadThumbnails();

            var mediaType = (string)((ComboBoxItem)cbMediaType.SelectedValue).Content;
            var quality = (mediaType != "mp3") ? (string)((ComboBoxItem)cbFormats.SelectedValue).Content : string.Empty;
            var mediaPath = workingPath + $"\\{selectedVideoDetail.id}.{mediaType}";

            try
            {
                isDownloadingVideo = true;
                if (File.Exists(mediaPath)) File.Delete(mediaPath);
                await clientHelper.DownloadMedia(selectedVideoDetail.id, quality, mediaPath, mediaType);
            }
            catch (Exception ex)
            {
                // todo: handle error
            }

            isDownloadingVideo = false;
        }
    }

}
