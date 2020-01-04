using System.Collections.Generic;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics.Imaging;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;

namespace VideoEffects
{
    // needs to be in a WinRT component class otherwise "COMException with error messages Failed to activate video effect/Class not registered when trying to use the effect later."
    // https://english.r2d2rigo.es/2016/03/10/creating-custom-video-effects-in-uwp-apps/

    // video effects
    // https://stackoverflow.com/questions/46096108/how-to-take-a-snapshot-with-mediaelement-in-uwp
    public sealed class SnapshotVidoEffect : IBasicVideoEffect
    {

        private static SoftwareBitmap Snap;
        public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device)
        {

        }

        public void ProcessFrame(ProcessVideoFrameContext context)
        {
            var inputFrameBitmap = context.InputFrame.SoftwareBitmap;
            Snap = inputFrameBitmap;
        }

        public static SoftwareBitmap GetSnapShot()
        {
            return Snap;
        }
        public void Close(MediaEffectClosedReason reason)
        {

        }

        public void DiscardQueuedFrames()
        {

        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public IReadOnlyList<VideoEncodingProperties> SupportedEncodingProperties
        {
            get { return new List<VideoEncodingProperties>(); }
        }
        public MediaMemoryTypes SupportedMemoryTypes
        {
            get { return MediaMemoryTypes.Cpu; }
        }

        public bool TimeIndependent
        {
            get { return true; }
        }



        public void SetProperties(IPropertySet configuration)
        {

        }
    }
}
