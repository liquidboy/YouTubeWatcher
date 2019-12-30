using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Models.MediaStreams;

namespace YouTubeWatcher.YT
{

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
            else
            {
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

}
