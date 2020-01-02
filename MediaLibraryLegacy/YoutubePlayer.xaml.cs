using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
