using MediaLibraryLegacy.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls;
using SharedCode.SQLite;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VideoEffects;
using Windows.Graphics.Imaging;
using Windows.Media.Editing;
using Windows.Media.Effects;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace MediaLibraryLegacy
{
    public sealed partial class ImagesEditor : UserControl
    {
        private ObservableCollection<ViewImageEditorMetadata> snapshots;

        public ImagesEditor()
        {
            this.InitializeComponent();
        }
        public void InitialSetup()
        {
            BindDatasources();
            LoadVideo();
            LoadImageEditorMetadata();
        }

        private void BindDatasources() {
            snapshots = new ObservableCollection<ViewImageEditorMetadata>();
            icLibraryItems.ItemsSource = snapshots;
        }

        private async void LoadVideo() {
            if (DataContext is ViewMediaMetadata) {
                var viewMediaMetadata = (ViewMediaMetadata)DataContext;
                var mediaUri = new Uri($"{App.mediaPath}\\{viewMediaMetadata.YID}.{viewMediaMetadata.MediaType}", UriKind.Absolute);

                var videoFile = await StorageFile.GetFileFromPathAsync(mediaUri.OriginalString);
                MediaClip clip = await MediaClip.CreateFromFileAsync(videoFile);

                var videoEffectDefinition = new VideoEffectDefinition(typeof(SnapshotVidoEffect).FullName);
                clip.VideoEffectDefinitions.Add(videoEffectDefinition);

                MediaComposition compositor = new MediaComposition();
                compositor.Clips.Add(clip);
                mePlayer.SetMediaStreamSource(compositor.GenerateMediaStreamSource());
            }
        }

        private async void LoadImageEditorMetadata()
        {
            if (DataContext is ViewMediaMetadata)
            {
                var viewMediaMetadata = (ViewMediaMetadata)DataContext;
                var foundItems = DBContext.Current.RetrieveEntities<ImageEditorMetadata>($"MediaUid='{viewMediaMetadata.UniqueId.ToString()}'");
                var orderedItems = foundItems.OrderBy(x => x.Number);

                var mediaPathFolder = await StorageFolder.GetFolderFromPathAsync(App.mediaPath);

                try
                {
                    var yidFolder = await mediaPathFolder.GetFolderAsync(viewMediaMetadata.YID);

                    foreach (var foundItem in orderedItems)
                    {
                        var bitmap = yidFolder != null ? await GetSoftwareBitmap(await yidFolder.GetFileAsync($"{viewMediaMetadata.YID}-{foundItem.Number}.jpg")) : null;

                        var source = bitmap == null ? null : new SoftwareBitmapSource();
                        if (source != null) await source.SetBitmapAsync(bitmap);

                        snapshots.Add(new ViewImageEditorMetadata()
                        {
                            Bitmap = bitmap,
                            Source = source,
                            Number = foundItem.Number,
                            Position = TimeSpan.FromSeconds(foundItem.TotalSeconds)
                        });
                    }
                }
                catch { 
                    // todo: possibly yid folder does not exist find a nicer way to check if folder exists
                }
                
            }
        }

        private async Task<SoftwareBitmap> GetSoftwareBitmap(StorageFile inputFile) {

            SoftwareBitmap softwareBitmap;

            using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            }

            return FixBitmapForBGRA8(softwareBitmap);
        }

        private SoftwareBitmap FixBitmapForBGRA8(SoftwareBitmap bitmap) {
            // SoftwareBitmapSource::SetBitmapAsync only supports SoftwareBitmap with positive width/height, bgra8 pixel format and pre-multiplied or no alpha.'

            if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || bitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
            {
                return SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }
            return bitmap;
        }

        private async void TakeSnapshot(object sender, RoutedEventArgs e)
        {
            var bitmap = FixBitmapForBGRA8(SnapshotVidoEffect.GetSnapShot());
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(bitmap);

            var newSnapshot = new ViewImageEditorMetadata();
            newSnapshot.Bitmap = bitmap;
            newSnapshot.Source = source;
            newSnapshot.Number = snapshots.Count + 1;
            var pos = mePlayer.Position;
            newSnapshot.Position = new TimeSpan(pos.Days, pos.Hours, pos.Minutes, pos.Seconds); ;

            snapshots.Add(newSnapshot);

            SendSystemNotification("Snapshot taken!");
        }

        private async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap, StorageFile outputFile)
        {
            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Set properties
                var propertySet = new BitmapPropertySet();
                var qualityValue = new BitmapTypedValue(
                    1.0, // Maximum quality
                    Windows.Foundation.PropertyType.Single
                    );
                propertySet.Add("ImageQuality", qualityValue);

                // Create an encoder with the desired format
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream, propertySet);

                // Set the software bitmap
                encoder.SetSoftwareBitmap(softwareBitmap);

                //// Set additional encoding parameters, if needed
                //encoder.BitmapTransform.ScaledWidth = 320;
                //encoder.BitmapTransform.ScaledHeight = 240;
                //encoder.BitmapTransform.Rotation = Windows.Graphics.Imaging.BitmapRotation.Clockwise90Degrees;
                //encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
                encoder.IsThumbnailGenerated = true;

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception err)
                {
                    const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);
                    switch (err.HResult)
                    {
                        case WINCODEC_ERR_UNSUPPORTEDOPERATION:
                            // If the encoder does not support writing a thumbnail, then try again
                            // but disable thumbnail generation.
                            encoder.IsThumbnailGenerated = false;
                            break;
                        default:
                            throw;
                    }
                }

                //throwing error?
                if (encoder.IsThumbnailGenerated == false)
                {
                    await encoder.FlushAsync();
                }
            }
        }

        private void DeleteSnapshot(object sender, RoutedEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is ViewImageEditorMetadata)
            {
                var viewImageEditorMetadata = (ViewImageEditorMetadata)((FrameworkElement)sender).DataContext;
                snapshots.Remove(viewImageEditorMetadata);
                UpdateSnapshotNumbers();
            }
        }

        private void UpdateSnapshotNumbers() {
            for (int i = 0; i < snapshots.Count; i++)
            {
                var snapshot = snapshots[i];
                snapshot.Number = i + 1;
                snapshots[i] = snapshot;
            }
        }

        private void TabChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ClearDemoTiles();
                ctlStyleizeImage.UnloadImage();
                var tvi = (TabViewItem)e.AddedItems[0];
                switch (tvi.Header)
                {
                    case "Resize Snapshot":
                        mePlayer.Pause();
                        grdStylizeSnapshot.Visibility = Visibility.Collapsed;
                        grdMediaEditor.Visibility = Visibility.Collapsed;
                        grdTileEditor.Visibility = Visibility.Collapsed;
                        grdImageEditor.Visibility = Visibility.Visible;
                        break;
                    case "Stylize Snapshot":
                        mePlayer.Pause();
                        grdMediaEditor.Visibility = Visibility.Collapsed;
                        grdTileEditor.Visibility = Visibility.Collapsed;
                        grdImageEditor.Visibility = Visibility.Collapsed;
                        grdStylizeSnapshot.Visibility = Visibility.Visible;
                        ctlStyleizeImage.LoadImage();
                        break;
                    case "Take Snapshots":
                        grdStylizeSnapshot.Visibility = Visibility.Collapsed;
                        grdImageEditor.Visibility = Visibility.Collapsed;
                        grdTileEditor.Visibility = Visibility.Collapsed;
                        grdMediaEditor.Visibility = Visibility.Visible;
                        break;
                    case "Tiles":
                        mePlayer.Pause();
                        CreateDemoTiles();
                        grdStylizeSnapshot.Visibility = Visibility.Collapsed;
                        grdImageEditor.Visibility = Visibility.Collapsed;
                        grdMediaEditor.Visibility = Visibility.Collapsed;
                        grdTileEditor.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        private async void SaveSnapshots(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewMediaMetadata)
            {
                SendSystemNotification("Saving all snapshots ...");
                ClearDemoTiles();

                var viewMediaMetadata = (ViewMediaMetadata)DataContext;

                var mediaPathFolder = await StorageFolder.GetFolderFromPathAsync(App.mediaPath);
                if (mediaPathFolder != null)
                {
                    // if folder exists delete it 
                    await StorageHelper.TryDeleteFolder(viewMediaMetadata.YID, mediaPathFolder);

                    // delete DB ImageMetadata's if they exist
                    EntitiesHelper.DeleteAllImageEditorMetadata(viewMediaMetadata.UniqueId);

                    // create folder 
                    var yidFolder = await mediaPathFolder.CreateFolderAsync(viewMediaMetadata.YID);

                    foreach (var snapshot in snapshots) {

                        // save each snapshot as an image into the new folder
                        var newSnapshotFile = await yidFolder.CreateFileAsync($"{viewMediaMetadata.YID}-{snapshot.Number}.jpg");
                        SaveSoftwareBitmapToFile(snapshot.Bitmap, newSnapshotFile);

                        // create DB record for each snapshot    
                        EntitiesHelper.AddImageEditorMetadata(viewMediaMetadata.UniqueId, snapshot.Number, snapshot.Position.TotalSeconds);
                    }

                    SendSystemNotification("Snapshots saved!");
                    CreateDemoTiles();
                }
            }
        }
        private void ClearDemoTiles() => spTilesDemo.Children.Clear();
        private void CreateDemoTiles() {
            var tile1 = new SnapshotsTile() { Width = 250, Height = 250, Direction = RotatorTile.RotateDirection.Down };
            spTilesDemo.Children.Add(tile1);
            var sp = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            var tile2 = new SnapshotsTile() { Width = 250, Height = 140, Direction = RotatorTile.RotateDirection.Left, Margin = new Thickness(10) };
            sp.Children.Add(tile2);
            var tile3 = new SnapshotsTile() { Width = 140, Height = 140, Direction = RotatorTile.RotateDirection.Up, Margin= new Thickness(10)};
            sp.Children.Add(tile3);
            spTilesDemo.Children.Add(sp);

        }

        private void SendSystemNotification(string message, int duration = 2000) => systemNotifications.Show(message, duration);

        private void SnapshotSelected(object sender, PointerRoutedEventArgs e)
        {
            var viewImageEditorMetadata = (ViewImageEditorMetadata)((Image)sender).DataContext;
            grdImageEditor.DataContext = viewImageEditorMetadata;
            ctlStyleizeImage.DataContext = viewImageEditorMetadata;

            tvMain.SelectedIndex = 1;
        }

        private async void SaveEdit(object sender, RoutedEventArgs e)
        {
            if (grdImageEditor.DataContext != null && grdImageEditor.DataContext is ViewImageEditorMetadata) {
                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    // get bits from edited image
                    await imgCropper.SaveAsync(stream, BitmapFileFormat.Jpeg, true);
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                    var editedBitmap = await decoder.GetSoftwareBitmapAsync();

                    // create bitmaps for persisting in models
                    var source = new SoftwareBitmapSource();
                    await source.SetBitmapAsync(FixBitmapForBGRA8(editedBitmap));

                    // store bitmap in models
                    var currentSelectedSnapshot = (ViewImageEditorMetadata)grdImageEditor.DataContext;
                    currentSelectedSnapshot.Bitmap = editedBitmap;
                    currentSelectedSnapshot.Source = source;   

                    // force col to be updated
                    for (int i = 0; i < snapshots.Count(); i++)
                    {
                        if (snapshots[i].Number == currentSelectedSnapshot.Number) {
                            snapshots[i] = currentSelectedSnapshot;
                            break;
                        }
                    }

                    SendSystemNotification("Snapshot updated!");
                }
            }
        }
    }
}

// software bitmap
// https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/imaging
