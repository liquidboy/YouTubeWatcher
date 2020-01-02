using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MediaLibraryLegacy
{
    public sealed partial class YoutubePlayer : UserControl
    {
        public event EventHandler<MediaChangedEventArgs> MediaChanged;
        WebView wvMain;

        public YoutubePlayer()
        {
            this.InitializeComponent();
        }

        public void InitialSetup() {
            wvMain = new WebView(WebViewExecutionMode.SeparateProcess);
            wvMain.ContentLoading += Wv_ContentLoading;
            layoutRoot.Children.Add(wvMain);
            Hide();
        }

        private string lastProcessedUrl;

        private async void Wv_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
            var url = await wvMain.InvokeScriptAsync("eval", new string[] { "document.location.href;" });
            if (HasUrlBeenProcessed(url)) return;

            lastProcessedUrl = url;

            MediaChanged?.Invoke(null, new MediaChangedEventArgs() { MediaUri = new Uri(url) });

            //tbUrl.Text = url;
            //cbFormats.Items.Clear();
            //spDownloadToolbar.Visibility = Visibility.Collapsed;

            //if (IsValidUrl(url))
            //{
            //    var videoDetail = await GetVideoDetails(url);
            //    if (videoDetail != null)
            //    {
            //        foreach (var mt in videoDetail.qualities)
            //        {
            //            cbFormats.Items.Add(new ComboBoxItem() { Content = mt });
            //        }
            //    }
            //    ShouldWeShowToolbar();
            //    await DownloadMediumThumbnail(videoDetail);
            //}
        }

        private bool HasUrlBeenProcessed(string urlToProcess) => urlToProcess.Equals(lastProcessedUrl, StringComparison.CurrentCultureIgnoreCase);

        public void LoadUri(Uri uri) {
            Show();
            wvMain.Navigate(uri);
        }

        public void Hide() {
            //wv.Source = null;
            wvMain.Visibility = Visibility.Collapsed;
        }

        public void Show() {
            wvMain.Visibility = Visibility.Visible;
        }
    }

    public class MediaChangedEventArgs : EventArgs {
        public Uri MediaUri;
    }
}
