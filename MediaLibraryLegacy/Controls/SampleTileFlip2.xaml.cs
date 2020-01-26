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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace MediaLibraryLegacy.Controls
{
    
    public sealed partial class SampleTileFlip2 : UserControl
    {
        bool isFlip = true;
        
        public SampleTileFlip2()
        {
            this.InitializeComponent();
            sbMain.Begin();
        }

        private void sbMain_Completed(object sender, object e)
        {
            sbMain.Stop();
            if (isFlip) {
                image2.Source = new BitmapImage(new Uri("ms-appx:///Assets/1.jpg"));
                image1.Source = new BitmapImage(new Uri("ms-appx:///Assets/2.jpg"));
            }
            else {
                image1.Source = new BitmapImage(new Uri("ms-appx:///Assets/1.jpg"));
                image2.Source = new BitmapImage(new Uri("ms-appx:///Assets/2.jpg"));
                
            }
            isFlip = !isFlip;
            sbMain.BeginTime = TimeSpan.FromSeconds(0);
            sbMain.Begin();
        }



        public int TileSize
        {
            get { return (int)GetValue(TileSizeProperty); }
            set { SetValue(TileSizeProperty, value); }
        }

        public static readonly DependencyProperty TileSizeProperty =
            DependencyProperty.Register("TileSize", typeof(int), typeof(SampleTileFlip2), new PropertyMetadata(200));


    }
}
