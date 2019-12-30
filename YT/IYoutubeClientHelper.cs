using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeWatcher.YT
{

    public interface IYoutubeClientHelper
    {
        public Task<VideoDetails> GetVideoMetadata(string videoId);
        public string GetVideoID(string videoUrl);
        public Task DownloadMedia(string id, string quality, string videoPath, string mediaType);
    }

}
