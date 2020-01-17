using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;

namespace SharedCodeUWP.ImageLoader
{
    internal class TextDrawer : IContentDrawer
    {
        private string _text;
        private CanvasTextFormat _textFormat;
        private Color _textColor;
        private Color _backgroundColor;

        public TextDrawer(string text, CanvasTextFormat textFormat, Color textColor, Color bgColor)
        {
            _text = text;
            _textFormat = textFormat;
            _textColor = textColor;
            _backgroundColor = bgColor;
        }

#pragma warning disable 1998
        public async Task Draw(CompositionGraphicsDevice device, Object drawingLock, CompositionDrawingSurface surface, Size size)
        {
            using (var ds = CanvasComposition.CreateDrawingSession(surface))
            {
                ds.Clear(_backgroundColor);
                ds.DrawText(_text, new Rect(0, 0, surface.Size.Width, surface.Size.Height), _textColor, _textFormat);
            }
        }
    }
}
