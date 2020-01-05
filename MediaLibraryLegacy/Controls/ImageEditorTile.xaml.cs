using SharedCode.SQLite;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MediaLibraryLegacy.Controls
{
    public sealed partial class ImageEditorTile : UserControl
    {
        private ObservableCollection<ViewRotatingTile> tileImages;

        public ImageEditorTile()
        {
            this.InitializeComponent();
            tileImages = new ObservableCollection<ViewRotatingTile>();
            tileTest.ItemsSource = tileImages;
        }

        public void InitialSetup(ViewMediaMetadata viewMediaMetadata)
        {
            LoadThumbnails(viewMediaMetadata);
        }

        private void LoadThumbnails(ViewMediaMetadata viewMediaMetadata)
        {
            tileImages.Clear();

            var foundItems = DBContext.Current.RetrieveEntities<ImageEditorMetadata>($"MediaUid='{viewMediaMetadata.UniqueId.ToString()}'");
            var orderedItems = foundItems.OrderBy(x => x.Number);

            foreach (var foundItem in orderedItems)
            {
                tileImages.Add(new ViewRotatingTile()
                {
                    Thumbnail = new Uri($"{App.mediaPath}\\{viewMediaMetadata.YID}\\{viewMediaMetadata.YID}-{foundItem.Number}.jpg", UriKind.Absolute)
                });
            }
            tileTest.Visibility = (orderedItems.Count() > 0) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
