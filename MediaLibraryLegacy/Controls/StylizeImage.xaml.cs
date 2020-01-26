using Microsoft.Graphics.Canvas.Effects;
using SharedCodeUWP.ImageLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MediaLibraryLegacy.Controls
{
    public sealed partial class StylizeImage : UserControl
    {
        private Compositor m_compositor;
        private ContainerVisual m_root;
        private SpriteVisual m_sprite;

        private CompositionSurfaceBrush m_noEffectBrush;
        private CompositionEffectBrush m_exposureEffectBrush;
        private CompositionEffectBrush m_grayscaleEffectBrush;

        private double m_imageAspectRatio;

        public EventHandler OnSave;

        public StylizeImage()
        {
            this.InitializeComponent();
        }

        public void LoadImage() {
            InitializePipeline();
            InitializeBrushes();
            SetRenderedBrush(m_noEffectBrush);
        }



        public void UnloadImage() {

        }

        private void InitializeBrushes() {
            if (this.DataContext is ViewImageEditorMetadata)
            {
                var viewImageEditorMetadata = (ViewImageEditorMetadata)this.DataContext;

                // noeffect brush
                Size imageSize;
                //m_noEffectBrush = CreateBrushFromAsset("xxxxx.jpg", out imageSize);
                m_noEffectBrush = CreateBrushFromAsset(viewImageEditorMetadata.Bitmap, out imageSize);
                m_imageAspectRatio = (imageSize.Width == 0 && imageSize.Height == 0) ? 1 : imageSize.Width / imageSize.Height;

                renderSurface.UpdateLayout();
                ResizeImage(new Size(renderSurface.ActualWidth, renderSurface.ActualHeight));

                // Exposure
                var exposureEffectDesc = new ExposureEffect
                {
                    Name = "effect",
                    Source = new CompositionEffectSourceParameter("Image")
                };
                m_exposureEffectBrush = m_compositor.CreateEffectFactory(exposureEffectDesc, new[] { "effect.Exposure" }).CreateBrush();
                ChangeExposureValue(0.5f);
                m_exposureEffectBrush.SetSourceParameter("Image", m_noEffectBrush);

                // monochromatic gray
                var grayscaleEffectDesc = new GrayscaleEffect
                {
                    Name = "effect",
                    Source = new CompositionEffectSourceParameter("Image")
                };
                m_grayscaleEffectBrush = m_compositor.CreateEffectFactory( grayscaleEffectDesc ).CreateBrush();
                m_grayscaleEffectBrush.SetSourceParameter("Image", m_noEffectBrush);
            }
        }

        private void ChangeExposureValue(float exposure) => m_exposureEffectBrush.Properties.InsertScalar("effect.Exposure", exposure);

        private void SetRenderedBrush(CompositionBrush brushToRender) {
            m_sprite.Brush = brushToRender; //m_effectBrushes[(int)m_activeEffectType];
        }

        private CompositionSurfaceBrush CreateBrushFromAsset(string name, out Size size)
        {
            CompositionDrawingSurface surface = ImageLoader.Instance.LoadFromUri(new Uri("ms-appx:///Assets/" + name)).Surface;
            size = surface.Size;
            return m_compositor.CreateSurfaceBrush(surface);
        }

        private CompositionSurfaceBrush CreateBrushFromAsset(SoftwareBitmap softwareBitmap, out Size size)
        {
            CompositionDrawingSurface surface = ImageLoader.Instance.LoadFromSoftwareBitmap(softwareBitmap, Size.Empty, null).Surface;
            size = surface.Size;
            return m_compositor.CreateSurfaceBrush(surface);
        }

        private void InitializePipeline() {
            m_compositor = ElementCompositionPreview.GetElementVisual(renderSurface).Compositor;
            m_root = m_compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(renderSurface, m_root);

            ImageLoader.Initialize(m_compositor);

            m_sprite = m_compositor.CreateSpriteVisual();
            m_root.Children.InsertAtTop(m_sprite);
        }

        private void ResizeImage(Size windowSize)
        {
            double visibleWidth = windowSize.Width; // - EffectControls.Width;
            double visibleHeight = windowSize.Height;
            double newWidth = visibleWidth;
            double newHeight = visibleHeight;

            newWidth = newHeight * m_imageAspectRatio;
            if (newWidth > visibleWidth)
            {
                newWidth = visibleWidth;
                newHeight = newWidth / m_imageAspectRatio;
            }

            m_sprite.Offset = new Vector3(
                //(float)(EffectControls.Width + (visibleWidth - newWidth) / 2),
                (float)((visibleWidth - newWidth) / 2),
                (float)((visibleHeight - newHeight) / 2),
                0.0f);
            m_sprite.Size = new Vector2(
                (float)newWidth,
                (float)newHeight);
        }

        private void SaveVisualToFile() {
            // 


        }

        private void butChangeEffect(object sender, RoutedEventArgs e)
        {
            var but = (Button)sender;
            switch (but.Content) {
                case "exposure": SetRenderedBrush(m_exposureEffectBrush); break;
                case "grayscale": SetRenderedBrush(m_grayscaleEffectBrush); break;
                default:
                    SetRenderedBrush(m_noEffectBrush);
                    break;
            }
        }

        private void butSave_Click(object sender, RoutedEventArgs e)
        {
            OnSave?.Invoke(null, null);
        }
    }
}


// fast vs slow effects
// https://docs.microsoft.com/en-us/windows/uwp/composition/composition-tailoring