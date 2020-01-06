using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace MediaLibraryLegacy.Converters
{

    public class SoftwareBitmapToWriteableBitmap : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            try {
                var softwareBitmap = (SoftwareBitmap)value;
                WriteableBitmap bitmap = new WriteableBitmap(softwareBitmap.PixelWidth, softwareBitmap.PixelHeight);
                softwareBitmap.CopyToBuffer(bitmap.PixelBuffer);
                return bitmap;
            }
            catch { }
            return null;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BytesToDisplaySize : IValueConverter
    {
        private static readonly string[] Units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value is null)
                return default(string);

            double size = (long)value;
            var unit = 0;

            while (size >= 1024)
            {
                size /= 1024;
                ++unit;
            }

            return $"{size:0.#} {Units[unit]}";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TimeSpanFormat : IValueConverter
    {
        public string StringFormat { get; set; }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string result = "";
            if (value == null)
            {
                return null;
            }

            if (parameter == null)
            {
                return value;
            }

            if (value is TimeSpan timeSpan)
            {
                try
                {
                    result = timeSpan.ToString(StringFormat);
                }
                catch (Exception e)
                {
                    result = "";
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class AbsoluteUriToImageSource : IValueConverter
    {
        
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return new Windows.UI.Xaml.Media.Imaging.BitmapImage((Uri)value);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
