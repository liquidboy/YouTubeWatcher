using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Hosting;

namespace MediaLibraryLegacy
{
    public static class WindowHelper
    {
        public const double DefaultEditorWindowWidth = 1950;
        public const double DefaultEditorWindowHeight = 1350;

        // todo: work out a huge mem leak, the windowContent is still running even when we destroy the appWindow ????
        // i.e. the mediaPlayerElement is still running , i can here the media playing
        public static async void OpenWindow(UIElement windowContent, double width, double height, Action finishedOpeningAction) {
            AppWindow appWindow = await AppWindow.TryCreateAsync();
            appWindow.RequestSize(new Size(width, height));

            Grid appWindowRootGrid = new Grid();
            appWindowRootGrid.Children.Add(windowContent);

            ElementCompositionPreview.SetAppWindowContent(appWindow, windowContent);

            appWindow.Closed += (a, o) =>
            {
                appWindowRootGrid.Children.Remove(windowContent);
                appWindowRootGrid = null;
                
                windowContent = null;
                appWindow = null;
                
            };

            await appWindow.TryShowAsync();
            finishedOpeningAction?.Invoke();
        }
    }
}
