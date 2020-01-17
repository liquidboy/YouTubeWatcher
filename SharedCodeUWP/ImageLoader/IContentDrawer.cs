using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;

using Microsoft.Graphics.Canvas;

namespace SharedCode.ImageLoader
{
    public interface IContentDrawer
    {
        Task Draw(CompositionGraphicsDevice device, Object drawingLock, CompositionDrawingSurface surface, Size size);
    }

    public delegate void LoadTimeEffectHandler(CompositionDrawingSurface surface, CanvasBitmap bitmap, CompositionGraphicsDevice device);
}
