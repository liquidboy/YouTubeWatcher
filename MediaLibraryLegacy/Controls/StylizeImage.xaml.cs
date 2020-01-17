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
using Windows.UI.Xaml.Navigation;

namespace MediaLibraryLegacy.Controls
{
    public sealed partial class StylizeImage : UserControl
    {
        private Compositor m_compositor;
        private ContainerVisual m_root;
        private SpriteVisual m_sprite;

        private CompositionSurfaceBrush m_noEffectBrush;
        private double m_imageAspectRatio;

        public StylizeImage()
        {
            this.InitializeComponent();
        }

        public void LoadImage() {
            if (this.DataContext is ViewImageEditorMetadata) {
                var viewImageEditorMetadata = (ViewImageEditorMetadata)this.DataContext;

                InitializeCompBits();

                Size imageSize;
                m_noEffectBrush = CreateBrushFromAsset(viewImageEditorMetadata.Bitmap , out imageSize);
                m_imageAspectRatio = (imageSize.Width == 0 && imageSize.Height == 0) ? 1 : imageSize.Width / imageSize.Height;

                m_sprite = m_compositor.CreateSpriteVisual();
                ResizeImage(new Size(layoutRoot.ActualWidth, layoutRoot.ActualHeight));
                m_root.Children.InsertAtTop(m_sprite);
            }
        }

        public void UnloadImage() {

        }

        private CompositionSurfaceBrush CreateBrushFromAsset(SoftwareBitmap softwareBitmap, out Size size)
        {
            CompositionDrawingSurface surface = ImageLoader.Instance.LoadFromSoftwareBitmap(softwareBitmap, Size.Empty, null).Surface;
            size = surface.Size;
            return m_compositor.CreateSurfaceBrush(surface);
        }

        private void InitializeCompBits() {
            m_compositor = ElementCompositionPreview.GetElementVisual(layoutRoot).Compositor;
            m_root = m_compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(layoutRoot, m_root);

            ImageLoader.Initialize(m_compositor);
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
    }
}
