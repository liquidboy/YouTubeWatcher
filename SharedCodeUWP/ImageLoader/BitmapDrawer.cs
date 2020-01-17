using System;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.UI.Composition;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Composition;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;


namespace SharedCodeUWP.ImageLoader
{
    public class BitmapDrawer : IContentDrawer
    {
        Uri _uri;
        LoadTimeEffectHandler _handler;
        StorageFile _file;

        public BitmapDrawer(Uri uri, LoadTimeEffectHandler handler)
        {
            _uri = uri;
            _handler = handler;
        }

        public BitmapDrawer(StorageFile file, LoadTimeEffectHandler handler)
        {
            _file = file;
            _handler = handler;
        }

        public Uri Uri
        {
            get { return _uri; }
        }


        private async Task<SoftwareBitmap> LoadFromFile(StorageFile file)
        {
            SoftwareBitmap softwareBitmap;

            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Premultiplied);
            }

            return softwareBitmap;
        }

        public async Task Draw(CompositionGraphicsDevice device, Object drawingLock, CompositionDrawingSurface surface, Size size)
        {
            var canvasDevice = CanvasComposition.GetCanvasDevice(device);

            CanvasBitmap canvasBitmap;
            if (_file != null)
            {
                SoftwareBitmap softwareBitmap = await LoadFromFile(_file);
                canvasBitmap = CanvasBitmap.CreateFromSoftwareBitmap(canvasDevice, softwareBitmap);
            }
            else
            {
                canvasBitmap = await CanvasBitmap.LoadAsync(canvasDevice, _uri);
            }


            var bitmapSize = canvasBitmap.Size;

            //
            // Because the drawing is done asynchronously and multiple threads could
            // be trying to get access to the device/surface at the same time, we need
            // to do any device/surface work under a lock.
            //
            lock (drawingLock)
            {
                Size surfaceSize = size;
                if (surface.Size != size || surface.Size == new Size(0, 0))
                {
                    // Resize the surface to the size of the image
                    CanvasComposition.Resize(surface, bitmapSize);
                    surfaceSize = bitmapSize;
                }

                // Allow the app to process the bitmap if requested
                if (_handler != null)
                {
                    _handler(surface, canvasBitmap, device);
                }
                else
                {
                    // Draw the image to the surface
                    using (var session = CanvasComposition.CreateDrawingSession(surface))
                    {
                        session.Clear(Windows.UI.Color.FromArgb(0, 0, 0, 0));
                        session.DrawImage(canvasBitmap, new Rect(0, 0, surfaceSize.Width, surfaceSize.Height), new Rect(0, 0, bitmapSize.Width, bitmapSize.Height));
                    }
                }
            }
        }
    }
}
