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
        public DirectNPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;

            viewModel.ViewPage = new WeakReference<Page>(this);
            viewModel.ViewSwapChainPanel = new WeakReference<SwapChainPanel>(_scp);
        }

        private void DirectNPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (viewModel.PageRenderer != null)
            {
                this.UpdateLayout();
                try
                {
                    viewModel.PageRenderer.Initialize((uint)_scp.ActualWidth, (uint)_scp.ActualHeight);
                }
                catch
                {
                    throw new InvalidProgramException("Error at initializsizng renderer.");
                }
                // move from here to Model/ViewModel
                viewModel.PageRenderer.SetSwapChainPanel(_scp);
                viewModel.SCPSize_Changed += viewModel.PageRenderer.Panel_SizeChanged;
                viewModel.PageRenderer.StartRendering();
            }
            else
            {
                throw new InvalidDataException("The last renderer is invalid.");
            }
        }
        private void DirectNPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // move from here to Model/ViewModel
            viewModel.PageRenderer.StopRendering();
            viewModel.SCPSize_Changed -= viewModel.PageRenderer.Panel_SizeChanged;
            viewModel.PageRenderer.SetSwapChainPanel(null);
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
            }
            e.Handled = true;
            viewModel.ShaderPanel_PointerMovedCommand.Execute(e);
        }

        private void SwapChainPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (m_can_get_point)
            {
                viewModel.PressedPoint = e.GetCurrentPoint(sender as SwapChainPanel).Position;
            }
            viewModel.ShaderPanel_PointerPressedCommand.Execute(e);
        }

        private void SwapChainPanel_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (m_can_get_point)
            {
                viewModel.ReleasedPoint = e.GetCurrentPoint(sender as SwapChainPanel).Position;
            }
            viewModel.ShaderPanel_PointerReleasedCommand.Execute(e);
        }
    }
}
