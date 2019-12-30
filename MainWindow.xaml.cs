using Microsoft.Toolkit.Wpf.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

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

            clientHelper = new YoutubeClientHelper(new YoutubeClient(), installLocation);

            Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlProcess process = new Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlProcess();
            //process.ProcessExited += Process_ProcessExited;
            wvMain = new WebView(process);
            wvMain.Margin = new Thickness(0, 0, 0, 30);
            wvMain.ContentLoading += WvMain_ContentLoading;
            layoutRoot.Children.Add(wvMain);
            wvMain.Source = new Uri("https://www.youtube.com");
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
        private async void butLoad_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValidUrl()) return;
            if (selectedVideoDetail == null) return;

            await DownloadThumbnails();

            var mediaType = (string)((ComboBoxItem)cbMediaType.SelectedValue).Content;
            var quality = (string)((ComboBoxItem)cbFormats.SelectedValue).Content;
            var mediaPath = workingPath + $"\\{selectedVideoDetail.id}.{mediaType}";

            try
            {
                if (File.Exists(mediaPath)) File.Delete(mediaPath);
                await clientHelper.DownloadMedia(selectedVideoDetail.id, quality, mediaPath, mediaType);
            }
            catch (Exception ex)
            {
                // todo: handle error
            }

            var stream = new MediaStream();
            var video = await clientHelper.GetVideoMetadata(selectedVideoDetail.id);
            var videoStream = await stream.prepareMediaStream(mediaPath); // No need to dispose MemoryStream, GC will take care of this
            //CleanDirectory.DeleteFile(videoDir, id + ".mp4");

        }
    }

    public class YoutubeClientHelper : IYoutubeClientHelper
    {
        IYoutubeClient client;
        IYoutubeConverter converter;

        public YoutubeClientHelper(IYoutubeClient client, string installLocation)
        {
            this.client = client;

            string ffmpegExePath = installLocation + "\\ffmpeg.exe"; //Path to the ffmpeg.exe file used to mux audio&video stream. It should be located in wwwrooot/ffmpeg.exe
            converter = new YoutubeConverter(client, ffmpegExePath);

        }

        public string GetVideoID(string videoUrl)
        {
            return YoutubeClient.ParseVideoId(videoUrl);
        }

        public async Task<VideoDetails> GetVideoMetadata(string videoId)
        {
            var video = await client.GetVideoAsync(videoId);
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(videoId);
            var qualities = SortQualities(streamInfoSet.GetAllVideoQualityLabels());

            return new VideoDetails() { id = videoId, ChannelName = video.Author, Title = video.Title, qualities = qualities, thumbnails = video.Thumbnails };
        }

        public async Task DownloadMedia(string id, string quality, string videoPath, string mediaType)
        {
            MediaStreamInfoSet streamInfoSet;
            streamInfoSet = await client.GetVideoMediaStreamInfosAsync(id);
            var audioStreamInfo = streamInfoSet.Audio.WithHighestBitrate();
            var videoStreamInfo = streamInfoSet.Video.FirstOrDefault(c => c.VideoQualityLabel == quality);

            if (mediaType == "mp4")
            {
                var mediaStreamInfos = new MediaStreamInfo[] { audioStreamInfo, videoStreamInfo };
                await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, videoPath, "mp4");
            }
            else if (mediaType == "mp3")
            {
                var mediaStreamInfos = new MediaStreamInfo[] { audioStreamInfo };
                await converter.DownloadAndProcessMediaStreamsAsync(mediaStreamInfos, videoPath, "mp3");
            }
            else {
                throw new ArgumentException("mediaType not supported");
            }
        }

        IEnumerable<string> SortQualities(IEnumerable<string> qualities)
        {
            var sortedStrings = qualities.ToList();
            return sortedStrings
                .Select(s => new { str = s, split = s.Split('p') })
                .OrderBy(x => int.Parse(x.split[0]))
                .ThenBy(x => x.split[1])
                .Select(x => x.str)
                .ToList();
        }
    }



    public class MediaStream
    {
        public async Task<MemoryStream> prepareMediaStream(string path)
        {
            if (!File.Exists(path))
                return null;
            var memory = new MemoryStream(); // No need to dispose MemoryStream, GC will take care of this

            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return memory;
        }
    }

    public class VideoDetails
    {
        public string id { get; set; }
        public string ChannelName { get; set; }
        public string Title { get; set; }
        public IEnumerable<string> qualities { get; set; }
        public ThumbnailSet thumbnails { get; set; }
    }

    public interface IYoutubeClientHelper
    {
        public Task<VideoDetails> GetVideoMetadata(string videoId);
        public string GetVideoID(string videoUrl);
        public Task DownloadMedia(string id, string quality, string videoPath, string mediaType);
    }
}
