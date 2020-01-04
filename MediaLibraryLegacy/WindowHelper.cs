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

        public static async void OpenWindow(UIElement windowContent, double width, double height) {
            AppWindow appWindow = await AppWindow.TryCreateAsync();
            appWindow.RequestSize(new Size(width, height));
            ElementCompositionPreview.SetAppWindowContent(appWindow, windowContent);

            appWindow.Closed += (a, o) =>
            {
                windowContent = null;
                appWindow = null;
            };

            await appWindow.TryShowAsync();
        }
    }
}
