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
    
    public sealed partial class SampleTileFlip3 : UserControl
    {
        bool isFlip = true;

        ImageBrush img1Brush;
        ImageBrush img2Brush;

        public SampleTileFlip3()
        {
            this.InitializeComponent();

            img1Brush = new ImageBrush();
            img1Brush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/1.jpg"));
            image1.Source = img1Brush.ImageSource;

            img2Brush = new ImageBrush();
            img2Brush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/2.jpg"));
            image2.Source = img2Brush.ImageSource;
        }

        private void sbRotateTile_Completed(object sender, object e)
        {
            sbRotateTile.Stop();
            if (isFlip)
            {
                image2.Source = img1Brush.ImageSource;
                image1.Source = img2Brush.ImageSource;
            }
            else
            {
                image1.Source = img1Brush.ImageSource;
                image2.Source = img2Brush.ImageSource;
            }
            isFlip = !isFlip;
            sbRotateTile.Begin();
        }

        private void grid_Loaded(object sender, RoutedEventArgs e)
        {
            sbRotateTile.Begin();
        }

        private int currentSize = 2;

        private void butToggleSize_Click(object sender, RoutedEventArgs e)
        {
            switch (currentSize) { 
                case 1:
                    TileSizeW = 400;
                    TileSizeHalfW = TileSizeW / 2;
                    TileSizeH = 400;
                    TileSizeHalfH = TileSizeH / 2;
                    break;
                case 2:
                    TileSizeW = 300;
                    TileSizeHalfW = TileSizeW / 2;
                    TileSizeH = 150;
                    TileSizeHalfH = TileSizeH / 2;
                    break;
                case 3:
                    TileSizeW = 120;
                    TileSizeHalfW = TileSizeW / 2;
                    TileSizeH = 120;
                    TileSizeHalfH = TileSizeH / 2;
                    break;
            }
            currentSize++;
            if (currentSize == 4) currentSize = 1;
        }



        public int TileSizeW
        {
            get { return (int)GetValue(TileSizeWProperty); }
            set { SetValue(TileSizeWProperty, value); }
        }

        public static readonly DependencyProperty TileSizeWProperty =
            DependencyProperty.Register("TileSizeW", typeof(int), typeof(SampleTileFlip3), new PropertyMetadata(400));

        public int TileSizeHalfW
        {
            get { return (int)GetValue(TileSizeHalfWProperty); }
            set { SetValue(TileSizeHalfWProperty, value); }
        }

        public static readonly DependencyProperty TileSizeHalfWProperty =
            DependencyProperty.Register("TileSizeHalfW", typeof(int), typeof(SampleTileFlip3), new PropertyMetadata(200));



        public int TileSizeH
        {
            get { return (int)GetValue(TileSizeHProperty); }
            set { SetValue(TileSizeHProperty, value); }
        }

        public static readonly DependencyProperty TileSizeHProperty =
            DependencyProperty.Register("TileSizeH", typeof(int), typeof(SampleTileFlip3), new PropertyMetadata(400));



        public int TileSizeHalfH
        {
            get { return (int)GetValue(TileSizeHalfHProperty); }
            set { SetValue(TileSizeHalfHProperty, value); }
        }

        public static readonly DependencyProperty TileSizeHalfHProperty =
            DependencyProperty.Register("TileSizeHalfH", typeof(int), typeof(SampleTileFlip3), new PropertyMetadata(200));





        //public int TileSizeW { get; set; } = 500;
        //public int TileSizeHalfW { get { return TileSizeW / 2; } }

        //public int TileSizeH { get; set; } = 500;
        //public int TileSizeHalfH { get { return TileSizeH / 2; } }






    }
}
