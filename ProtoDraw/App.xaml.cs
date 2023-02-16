using DirectNXAML.Model;
using DirectNXAML.Views;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using static PInvoke.User32;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DirectNXAML
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public DrawManagerBase DrawManager;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }
        IntPtr m_hwnd;
        public IntPtr Hwnd { get { return m_hwnd; } }
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();

            m_hwnd = WinRT.Interop.WindowNative.GetWindowHandle(m_window);
            SetWindowDetails(m_hwnd, 1366, 768);

            m_window.Activate();
        }
        private static void SetWindowDetails(IntPtr hwnd, int width, int height)
        {
            var dpi = GetDpiForWindow(hwnd);
            float scalingFactor = (float)dpi / 96;
            width = (int)(width * scalingFactor);
            height = (int)(height * scalingFactor);

            _ = SetWindowPos(hwnd, SpecialWindowHandles.HWND_TOP,
                                        0, 0, width, height,
                                        SetWindowPosFlags.SWP_NOMOVE);
            _ = SetWindowLong(hwnd,
                   WindowLongIndexFlags.GWL_STYLE,
                   (SetWindowLongFlags)(GetWindowLong(hwnd,
                      WindowLongIndexFlags.GWL_STYLE) &
                      ~(int)SetWindowLongFlags.WS_MINIMIZEBOX &
                      ~(int)SetWindowLongFlags.WS_MAXIMIZEBOX));
        }
        private Window m_window;
        public Window WindowCurrent { get => m_window; }    // window can be instanced prural times then this isnot static.
        public IntPtr hWndCurrent { get => m_hwnd; }

        Windows.Graphics.PointInt32 m_main_content_frame_position;
        public Windows.Graphics.PointInt32 FrameCurrent00 { get => m_main_content_frame_position; set => m_main_content_frame_position = value; }
    }
}
