using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace MediaLibraryLegacy
{
    public static class XamlHelper
    {
        public static void CloseFlyout(object sender) {
            if (sender is FlyoutPresenter)
            {
                var flyout = (FlyoutPresenter)sender;

                if (flyout.Parent is Popup)
                {
                    var popup = (Popup)flyout.Parent;
                    popup.IsOpen = false;
                }
            }
        }
    }
}
