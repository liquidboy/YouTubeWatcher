using SharedCode.SQLite;
using SharedCode.YT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using YoutubeExplode;

namespace MediaLibraryLegacy
{
    public sealed partial class Taskbar : UserControl
    {
        string mediaPath;
        private IYoutubeClientHelper clientHelper;
        private Queue<MediaJob> jobQueue = new Queue<MediaJob>();

        public Taskbar()
        {
            this.InitializeComponent();
        }

        public void InitialSetup(string mediapath)
        {
            mediaPath = mediapath;

            // initialize Youtube helpers
            clientHelper = new YoutubeClientHelper(new YoutubeClient(), System.AppDomain.CurrentDomain.BaseDirectory);
        }

        private void DownloadMedia(object sender, RoutedEventArgs e)
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

        private void UpdateStatus()
        {
            tbStatus.Text = (jobQueue.Count > 0) ? "downloading media" : string.Empty;
            tbStatusTitle.Text = (jobQueue.Count > 0) ? "job" : string.Empty;
            spStatus.Visibility = (jobQueue.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateStatusImage(VideoDetails videoDetails)
        {
            if (videoDetails == null)
            {
                imgStatus.Source = null;
                return;
            }

            var uri = new Uri($"{mediaPath}\\{videoDetails.id}-medium.jpg", UriKind.Absolute);
            imgStatus.Source = new BitmapImage(uri);
        }

        private void UpdateLibraryStatistics()
        {
            var foundItems = DBContext.Current.RetrieveAllEntities<MediaMetadata>();
            var libraryCount = (foundItems == null) ? 0 : foundItems.Count;
            tbLibraryCount.Text = libraryCount.ToString();
        }

        private void UpdateJobStatistics() => tbJobsCount.Text = jobQueue.Count.ToString();

        private async Task ProcessYoutubeVideo(VideoDetails videoDetails, string mediaType, string quality)
        {
            if (videoDetails == null) return;

            await DownloadThumbnails(videoDetails);

            var path = mediaPath + $"\\{videoDetails.id}.{mediaType}";

            try
            {
                if (File.Exists(path)) File.Delete(path);
                await clientHelper.DownloadMedia(videoDetails.id, quality, path, mediaType);
                var fileInfo = new FileInfo(path);
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
        private void ShowLibrary(object sender, RoutedEventArgs e)
        {

        }

        public async void MediaChanged(Uri media) {
            tbUrl.Text = media.OriginalString;
            cbFormats.Items.Clear();
            spDownloadToolbar.Visibility = Visibility.Collapsed;

            if (IsValidUrl(media.OriginalString))
            {
                var videoDetail = await GetVideoDetails(media.OriginalString);
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

        private async Task DownloadThumbnails(VideoDetails videoDetails)
        {
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
                using (var httpClient = new System.Net.Http.HttpClient()) {
                    // Get the file extension
                    var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                    var fileExtension = System.IO.Path.GetExtension(uriWithoutQuery);

                    // Download the image and write to the file
                    var path = System.IO.Path.Combine(mediaPath, $"{fileName}{fileExtension}");
                    var imageBytes = await httpClient.GetByteArrayAsync(uri);
                    await File.WriteAllBytesAsync(path, imageBytes);
                }
            }
            catch { }
        }

        private bool IsValidUrl(string ytUrl)
        {
            if (ytUrl == "https://www.youtube.com/") return false;
            if (string.IsNullOrEmpty(ytUrl)) return false;
            return true;
        }

        private void ShouldWeShowToolbar()
        {
            if (IsValidUrl(tbUrl.Text)) spDownloadToolbar.Visibility = Visibility.Visible;
            else spDownloadToolbar.Visibility = Visibility.Collapsed;
        }

        private async Task<VideoDetails> GetVideoDetails(string ytUrl)
        {
            if (!IsValidUrl(ytUrl)) return null;
            try
            {
                return await clientHelper.GetVideoMetadata(clientHelper.GetVideoID(ytUrl));
            }
            catch (Exception ex)
            {
                // todo: handle error
            }
            return null;
        }
    }


    public struct MediaJob
    {
        public VideoDetails VideoDetails;
        public string YoutubeUrl;
        public string MediaType;
        public string Quality;
    }
}
