using DirectNXAML.DrawData;
using DirectNXAML.Renderers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.IO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DirectNXAML.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DirectNPage : Page
    {
        //SwapChainPanel _scp = null;
        WeakReference<Page> m_page = null;
        WeakReference<SwapChainPanel> m_scp = null;
        public DirectNPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;

            m_page = new WeakReference<Page>(this);
            m_scp = new WeakReference<SwapChainPanel>(_scp);
        }

        private void DirectNPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (viewModel.PageRenderer != null)
            {
                try
                {
                    viewModel.PageRenderer.Initialize(800,800);
                }
                catch
                {
                    throw new InvalidProgramException("Error at initializsizng renderer.");
                }
                viewModel.PageRenderer.SetSwapChainPanel(_scp);
                viewModel.PageRenderer.StartRendering();
                this.HorizontalAlignment = HorizontalAlignment.Stretch;
                this.VerticalAlignment = VerticalAlignment.Stretch;
                this.UpdateLayout();
            }
            else
            {
                throw new InvalidDataException("The last renderer is invalid.");
            }
        }
        private void DirectNPage_Unloaded(object sender, RoutedEventArgs e)
        {
            viewModel.PageRenderer.StopRendering();
        }

        private void SetBG_White(object sender, RoutedEventArgs e)
        {
            viewModel.PageRenderer?.SetBGColor(1, 1, 1);
        }
        private void SetBG_Black(object sender, RoutedEventArgs e)
        {
            viewModel.PageRenderer?.SetBGColor(0, 0, 0);
        }
        bool m_can_get_point = false;
        private void SwapChainPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            m_can_get_point = true;
        }

        private void SwapChainPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            m_can_get_point = false;
        }

        private void SwapChainPanel_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            m_can_get_point = false;
        }

        private void SwapChainPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            viewModel.LocalWidth = e.NewSize.Width;
            viewModel.LocalHeight = e.NewSize.Height;
            viewModel.ActualWidth = (sender as SwapChainPanel).ActualWidth;
            viewModel.ActualHeight = (sender as SwapChainPanel).ActualHeight;
            viewModel.SwapChainActualSize = (sender as SwapChainPanel).ActualSize;
            viewModel.ShaderPanel_SizeChangedCommand.Execute(e);
        }

        private void SwapChainPanel_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (m_can_get_point)
            {
                viewModel.LocalPointerPoint = e.GetCurrentPoint(sender as SwapChainPanel).Position;
                viewModel.NormalizedPointerX = viewModel.LocalPointerPoint.X / viewModel.ActualWidth;
                viewModel.NormalizedPointerY = viewModel.LocalPointerPoint.Y / viewModel.ActualHeight;
            }
            viewModel.ShaderPanel_PointerMovedCommand.Execute(e);
        }

        private void SwapChainPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (m_can_get_point)
            {
                viewModel.NormalizedPressedPoint = e.GetCurrentPoint(sender as SwapChainPanel).Position;
                viewModel.NormalizedPressedX = viewModel.NormalizedPressedPoint.X / viewModel.ActualWidth;
                viewModel.NormalizedPressedY = viewModel.NormalizedPressedPoint.Y / viewModel.ActualHeight;
                viewModel.NormalizedPressedZ = Primitive.Fz;
            }
            viewModel.ShaderPanel_PointerPressedCommand.Execute(e);
        }

        private void SwapChainPanel_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (m_can_get_point)
            {
                viewModel.NormalizedReleasedPoint = e.GetCurrentPoint(sender as SwapChainPanel).Position;
                viewModel.NormalizedReleasedX = viewModel.NormalizedReleasedPoint.X / viewModel.ActualWidth;
                viewModel.NormalizedReleasedY = viewModel.NormalizedReleasedPoint.Y / viewModel.ActualHeight;
                viewModel.NormalizedReleasedZ = 0.0;
            }
            viewModel.ShaderPanel_PointerReleasedCommand.Execute(e);
        }
    }
}
