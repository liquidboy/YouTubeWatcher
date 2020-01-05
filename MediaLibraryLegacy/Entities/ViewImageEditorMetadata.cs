using System;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace MediaLibraryLegacy
{
    public struct ViewImageEditorMetadata
    {
        public SoftwareBitmap Bitmap { get; set; }
        public SoftwareBitmapSource Source { get; set; }
        public int Number { get; set; }
        public TimeSpan Position { get; set; }
    }
}
