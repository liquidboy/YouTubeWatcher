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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace MediaLibraryLegacy.Controls
{
    public sealed partial class SampleTileFlip : UserControl
    {
        bool isFlip = true;

        ImageBrush img1Brush;
        ImageBrush img2Brush;

        Storyboard sbToRun;

        TileSize bCurrentTileSize;

        public SampleTileFlip()
        {
            this.InitializeComponent();

            img1Brush = new ImageBrush();
            img1Brush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/1.jpg"));
            image1.Source = img1Brush.ImageSource;

            img2Brush = new ImageBrush();
            img2Brush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/2.jpg"));
            image2.Source = img2Brush.ImageSource;


            SetSize(TileSize.Small);
            sbToRun.Begin();
        }

        public enum TileSize { 
            Large,
            Small
        }

        private void SetSize(TileSize tileSize) {
            sbToRun?.Stop();
            bCurrentTileSize = tileSize;
            if (tileSize == TileSize.Large) {
                TileWidth = TileHeight = 400;
                sbToRun = sbMain400;
            }
            else if (tileSize == TileSize.Small) {
                TileWidth = TileHeight = 200;
                sbToRun = sbMain200;
            }
            sbToRun.Begin();
        }

        private void sbMain_Completed(object sender, object e)
        {
            sbToRun.Stop();
            if (isFlip) {
                image2.Source = img1Brush.ImageSource; //new BitmapImage(new Uri("ms-appx:///Assets/1.jpg"));
                image1.Source = img2Brush.ImageSource; //new BitmapImage(new Uri("ms-appx:///Assets/2.jpg"));
            }
            else {
                image1.Source = img1Brush.ImageSource; //new BitmapImage(new Uri("ms-appx:///Assets/1.jpg"));
                image2.Source = img2Brush.ImageSource; //new BitmapImage(new Uri("ms-appx:///Assets/2.jpg"));
            }
            isFlip = !isFlip;
            sbToRun.BeginTime = TimeSpan.FromSeconds(0);
            sbToRun.Begin();
        }



        public int TileWidth
        {
            get { return (int)GetValue(TileWidthProperty); }
            set { SetValue(TileWidthProperty, value); }
        }

        public static readonly DependencyProperty TileWidthProperty =
            DependencyProperty.Register("TileWidth", typeof(int), typeof(SampleTileFlip), new PropertyMetadata(400));



        public int TileHeight
        {
            get { return (int)GetValue(TileHeightProperty); }
            set { SetValue(TileHeightProperty, value); }
        }

        public static readonly DependencyProperty TileHeightProperty =
            DependencyProperty.Register("TileHeight", typeof(int), typeof(SampleTileFlip), new PropertyMetadata(400));

        private void butToggleSize_Click(object sender, RoutedEventArgs e)
        {
            if (bCurrentTileSize == TileSize.Small) SetSize(TileSize.Large);
            else if (bCurrentTileSize == TileSize.Large) SetSize(TileSize.Small);
        }
    }
}
