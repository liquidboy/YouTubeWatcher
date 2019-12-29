using Microsoft.Toolkit.Wpf.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace YouTubeWatcher
{
    public partial class MainWindow : Window
    {
        private IYoutubeClientHelper clientHelper;
        private string webRootPath = "d:\\deleteme";
        private string installLocation = System.AppDomain.CurrentDomain.BaseDirectory;
        private string mediaType = "mp4";
        private WebView wvMain;
        public MainWindow()
        {
            InitializeComponent();

            clientHelper = new YoutubeClientHelper(new YoutubeClient(), installLocation);

            Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlProcess process = new Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlProcess();
            wvMain = new WebView(process);
            wvMain.Margin = new Thickness(0, 0, 0, 30);
            layoutRoot.Children.Add(wvMain);

        }

        private async void butLoad_Click(object sender, RoutedEventArgs e)
        {
            wvMain.Navigate(new Uri(tbUrl.Text));

            VideoDetails details = await clientHelper.GetVideoMetadata(clientHelper.GetVideoID(tbUrl.Text));
            MediaStream stream = new MediaStream();

            string mediaPath = webRootPath + $"\\DownloadedVideos\\{details.id}.{mediaType}";

            try
            {
                if (File.Exists(mediaPath)) File.Delete(mediaPath);

                string quality = details.qualities.Last();
                await clientHelper.DownloadMedia(details.id, quality, mediaPath, mediaType);
            }
            catch (Exception ex)
            {
                // do something
            }

            var video = await clientHelper.GetVideoMetadata(details.id);
            MemoryStream videoStream = await stream.prepareMediaStream(mediaPath); // No need to dispose MemoryStream, GC will take care of this
            //CleanDirectory.DeleteFile(videoDir, id + ".mp4");

            //if (videoStream == null)
            //    return BadRequest();
            //return File(videoStream, "video/mp4", video.Title);
      
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
            MediaStreamInfoSet streamInfoSet = await client.GetVideoMediaStreamInfosAsync(videoId);
            IEnumerable<string> qualities = SortQualities(streamInfoSet.GetAllVideoQualityLabels());

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
            List<string> sortedStrings = qualities.ToList();
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
