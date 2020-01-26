using System;
using System.IO;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MediaLibraryLegacy
{
    sealed partial class App : Application
    {
        public static string mediaPath = "[filled in by code]";
        private const string mediaFolderName = "Media";
        public const string dbName = "youtubewatcher";
        public const string youtubeHomeUrl = "https://www.youtube.com";

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }


        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Ensure Media Folder Exists
            await EnsureMediaFolderExists();

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {

                }
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    //rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    rootFrame.Navigate(typeof(TestPage), e.Arguments);
                }
                Window.Current.Activate();
            }
        }

        // https://docs.microsoft.com/en-us/windows/uwp/design/app-settings/store-and-retrieve-app-data
        private async Task EnsureMediaFolderExists()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            //var myVideos = await StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Videos);
            var path = string.Empty;
            try
            {
                //var folderExists = await KnownFolders.VideosLibrary.GetFolderAsync("MediaLibraryLegacy");
                var folderExists = await localFolder.GetFolderAsync(mediaFolderName);
                path = folderExists.Path;
            }
            catch (Exception ex) {
                //var appFolder = await myVideos.SaveFolder.CreateFolderAsync("MediaLibraryLegacy", CreationCollisionOption.FailIfExists);
                var appFolder = await localFolder.CreateFolderAsync(mediaFolderName);
                path = appFolder.Path;
            }
            finally {
                mediaPath = path;
            }

            // ensure it has the right permission
            //SetFolderPermission(path);
        }

        private void SetFolderPermission(string path)
        {
            var directoryInfo = new System.IO.DirectoryInfo(path);
            var directorySecurity = directoryInfo.GetAccessControl();
            directorySecurity.AddAccessRule(new FileSystemAccessRule("ALL APPLICATION PACKAGES", FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.InheritOnly, AccessControlType.Allow));
            directoryInfo.SetAccessControl(directorySecurity);
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}
