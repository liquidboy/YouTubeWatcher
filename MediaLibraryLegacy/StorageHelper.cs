using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MediaLibraryLegacy
{
    public static class StorageHelper
    {
        public static async Task TryDeleteFile(string fileName, StorageFolder folder)
        {
            try
            {
                var foundFile = await folder.GetFileAsync(fileName);
                if (foundFile != null) await foundFile.DeleteAsync();
            }
            catch { }
        }

        public static async Task TryDeleteFolder(string folderName, StorageFolder folder)
        {
            try
            {
                var foundChildFolder = await folder.GetFolderAsync(folderName);
                var files = await foundChildFolder.GetFilesAsync();
                foreach (var file in files)
                {
                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }

                await foundChildFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch { }
        }

        public static async Task DownloadImageAsync(string fileName, Uri uri, string mediaPath)
        {
            try
            {
                using (var httpClient = new System.Net.Http.HttpClient())
                {
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
    }
}
