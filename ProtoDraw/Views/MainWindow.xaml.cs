using Microsoft.UI.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Graphics;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DirectNXAML.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var item = args.InvokedItemContainer as NavigationViewItem;
            if (args != null)
            {
                if ((string)(item.Tag) == "DirectNPage")
                {
                    ContentFrame.Navigate(typeof(DirectNPage), null);
                }
                else
                {
                    ContentFrame.Navigate(typeof(OtherPage), null);
                }
            }
        }


        private bool m_centered = false;
        /// <summary>
        /// Centralize Window
        /// https://stackoverflow.com/questions/74890047/how-can-i-set-my-winui3-program-to-be-started-in-the-center-of-the-screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (this.m_centered is false)
            {
                Center(this);
                m_centered = true;
            }
        }
        private static void Center(Window window)
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(window);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);

            if (AppWindow.GetFromWindowId(windowId) is AppWindow appWindow &&
                DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest) is DisplayArea displayArea)
            {
                PointInt32 CenteredPosition = appWindow.Position;
                CenteredPosition.X = (displayArea.WorkArea.Width - appWindow.Size.Width) / 2;
                CenteredPosition.Y = (displayArea.WorkArea.Height - appWindow.Size.Height) / 2;
                appWindow.Move(CenteredPosition);
            }
        }
    }
}
