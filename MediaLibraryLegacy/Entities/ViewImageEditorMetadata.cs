using System;
using Windows.UI.Xaml.Media.Imaging;

namespace MediaLibraryLegacy
{
    public struct ViewImageEditorMetadata
    {
        public SoftwareBitmapSource Source { get; set; }
        public int Number { get; set; }
        public TimeSpan Position { get; set; }
    }
}
