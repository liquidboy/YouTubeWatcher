using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace MediaLibraryLegacy
{
    public static class XamlHelper
    {
        public static void CloseFlyout(object sender) {
            var flyoutSender = FindParentFlyout(sender);
            if (flyoutSender is FlyoutPresenter)
            {
                var flyout = (FlyoutPresenter)flyoutSender;

                if (flyout.Parent is Popup)
                {
                    var popup = (Popup)flyout.Parent;
                    popup.IsOpen = false;
                }
            }
        }

        private static object FindParentFlyout(object sender)
        {
            if (sender == null) return null;
            else if (sender is FlyoutPresenter) return sender;
            else return FindParentFlyout(((FrameworkElement)sender).Parent);
        }
    }
}
