using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace MediaLibraryLegacy
{
    public static class FileFolderHelper
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
                await foundChildFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch { }
        }
    }
}
