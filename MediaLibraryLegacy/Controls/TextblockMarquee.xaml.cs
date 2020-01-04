using System.Timers;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MediaLibraryLegacy.Controls
{
    public sealed partial class TextblockMarquee : UserControl
    {
        Timer timer;

        public void SetText(string text) {
            tbMainText.Text = text;
            if (string.IsNullOrEmpty(text))
            {
                timer?.Stop();
                scrollviewer.ChangeView(0, scrollviewer.VerticalOffset, scrollviewer.ZoomFactor);
            }
            else {
                timer?.Start();
            }
        }

        public string GetText() { return tbMainText.Text; }

        public TextblockMarquee()
        {
            this.InitializeComponent();
            timer = new Timer();
        }

        private void scrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Elapsed += (ss, ee) => 
            {
                var runner = scrollviewer.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    scrollviewer.ChangeView(scrollviewer.HorizontalOffset + 2, scrollviewer.VerticalOffset, scrollviewer.ZoomFactor);
                    if (scrollviewer.HorizontalOffset == scrollviewer.ScrollableWidth) scrollviewer.ChangeView(0, scrollviewer.VerticalOffset, scrollviewer.ZoomFactor);
                });
            };
            timer.Interval = 150;
            //timer.Start();
        }


        private void scrollviewer_Unloaded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }
    }
}
