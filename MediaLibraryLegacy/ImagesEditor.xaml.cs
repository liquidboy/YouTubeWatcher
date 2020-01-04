using System;
using System.Collections.Generic;
using VideoEffects;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics.Imaging;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace MediaLibraryLegacy
{
    public sealed partial class ImagesEditor : UserControl
    {
        public ImagesEditor()
        {
            this.InitializeComponent();
        }
        public void InitialSetup() {
            LoadVideo();
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

        private async void TakeSnapshot(object sender, RoutedEventArgs e)
        {
            var bitmap = SnapshotVidoEffect.GetSnapShot();

            if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || bitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
            {
                bitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(bitmap);


            var newSnapshotImage = new Image();
            newSnapshotImage.Source = source;
            spImages.Children.Add(newSnapshotImage);
            newSnapshotImage.StartBringIntoView();
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
                //encoder.IsThumbnailGenerated = true;

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

                if (encoder.IsThumbnailGenerated == false)
                {
                    await encoder.FlushAsync();
                }
            }
        }
    }
}

// software bitmap
// https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/imaging