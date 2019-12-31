using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode.YT
{

    public interface IYoutubeClientHelper
    {
        Task<VideoDetails> GetVideoMetadata(string videoId);
        string GetVideoID(string videoUrl);
        Task DownloadMedia(string id, string quality, string videoPath, string mediaType);
    }

}
