using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DirectNXAML.DrawData;
using DirectNXAML.Model;
using DirectNXAML.Renderers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;
using System.Numerics;
using System.Windows.Input;

namespace DirectNXAML.ViewModels
{
    internal class DirectNPageViewModel : ObservableObject
	{
		Dx11Renderer m_renderer = null;
		// for a simple line drawing state transition
        enum ELineGetState : int
        {
            none = -1,
            Begin = 0,
			Pressed,
            Released,
            maxEnum
        }
        ELineGetState m_state = ELineGetState.none;
        private FLine3D m_lin;
        internal Dx11Renderer PageRenderer { get { return m_renderer; } set { m_renderer = value; } }
		int m_vertex_count = 0;
        public int VertexCount { get => m_vertex_count; set => SetProperty(ref m_vertex_count, value); }
		private void UpdateVertexCountDisplay() { VertexCount = ((App)Application.Current).DrawManager.VertexData.Length; }
        /// <summary>
        /// Constructor
        /// </summary>
        internal DirectNPageViewModel()
        {
            ((App)Application.Current).DrawManager = new SimpleDrawLineManager();
            m_renderer = new Dx11Renderer();

            SetState_DrawLineCommand += SetState_DrawLine;
            SetState_SelectCommand += SetState_Select;

            ShaderPanel_PointerMovedCommand = new RelayCommand<PointerRoutedEventArgs>(ShaderPanel_PointerMoved);
			ShaderPanel_PointerPressedCommand = new RelayCommand<PointerRoutedEventArgs>(ShaderPanel_PointerPressed);
			ShaderPanel_PointerReleasedCommand = new RelayCommand<PointerRoutedEventArgs>(ShaderPanel_PointerReleased);
            ColorData.ResetLineColor();
            UpdateVertexCountDisplay();
            m_state = ELineGetState.none;   // initial mode.
        }
        public void Dispose()
        {
            m_renderer.StopRendering();
        }

        internal RoutedEventHandler SetState_DrawLineCommand { get; private set; }
        private void SetState_DrawLine(object sender, RoutedEventArgs e)
		{
            if (m_state == ELineGetState.none)
            {
                m_state = ELineGetState.Begin;
            }
        }
        internal RoutedEventHandler SetState_SelectCommand { get; private set; }
        private void SetState_Select(object sender, RoutedEventArgs e)
        {
			m_state = ELineGetState.none;	// xxxx reset state machine first.
        }

        public ICommand ShaderPanel_PointerMovedCommand { get; private set; }
		private void ShaderPanel_PointerMoved(PointerRoutedEventArgs args)
		{
			args.Handled = true;

            if (m_state == ELineGetState.Pressed)
            {
                ((App)Application.Current).DrawManager.DelLastLine();
                m_lin.Ep.X = (float)NormalizedPointerX;
                m_lin.Ep.Y = (float)NormalizedPointerY;
                ((App)Application.Current).DrawManager.Add(m_lin);
                m_renderer.UpdateVertexBuffer();
            }
        }
		public ICommand ShaderPanel_PointerPressedCommand { get; private set; }
		private void ShaderPanel_PointerPressed(PointerRoutedEventArgs args)
		{
            args.Handled = true;

            if (m_state == ELineGetState.Begin)
            {
                ColorData.SetLine(ColorData.Blue);
                m_lin = new FLine3D();
                m_lin.Sp.X = m_lin.Ep.X = (float)NormalizedPointerX;
                m_lin.Sp.Y = m_lin.Ep.Y = (float)NormalizedPointerY;
                m_lin.SetCol(ColorData.Line);   // blue rubber
                ((App)Application.Current).DrawManager.Add(m_lin);
                UpdateVertexCountDisplay();
                m_renderer.UpdateVertexBuffer();
                m_state = ELineGetState.Pressed;
            }
        }

		public ICommand ShaderPanel_PointerReleasedCommand { get; private set; }
		private void ShaderPanel_PointerReleased(PointerRoutedEventArgs args)
		{
			args.Handled = true;

            if (m_state == ELineGetState.Pressed)
            {
                ColorData.SetLine(ColorData.White);
                ((App)Application.Current).DrawManager.DelLastLine();
                m_lin.Ep.X = (float)NormalizedPointerX;
                m_lin.Ep.Y = (float)NormalizedPointerY;
                m_lin.SetCol(ColorData.Line); // white : Rocked
                ((App)Application.Current).DrawManager.Add(m_lin);
                m_renderer.UpdateVertexBuffer();
                m_lin.Clear();
                m_state = ELineGetState.Begin;
            }
        }
		double m_local_height, m_local_width;
		internal double LocalHeight { get => m_local_height; set => SetProperty(ref m_local_height, value); }
		internal double LocalWidth { get => m_local_width; set => SetProperty(ref m_local_width, value); }

		Windows.Foundation.Point m_local_pointer;
		internal Windows.Foundation.Point LocalPointerPoint { get => m_local_pointer; set => SetProperty(ref m_local_pointer, value); }

		double m_nx, m_ny, m_nz = 0;
		internal double NormalizedPointerX { get => m_nx; set => SetProperty(ref m_nx, value); }
		internal double NormalizedPointerY { get => m_ny; set => SetProperty(ref m_ny, value); }
		internal double NormalizedPointerZ { get => m_nz; set => SetProperty(ref m_nz, value); }

		Windows.Foundation.Point m_pressed_pointer;
		internal Windows.Foundation.Point NormalizedPressedPoint { get => m_pressed_pointer; set => SetProperty(ref m_pressed_pointer, value); }
		
		double m_pressed_nx, m_pressed_ny, m_pressed_nz = 0;
		internal double NormalizedPressedX { get => m_pressed_nx; set => SetProperty(ref m_pressed_nx, value); }
		internal double NormalizedPressedY { get => m_pressed_ny; set => SetProperty(ref m_pressed_ny, value); }
		internal double NormalizedPressedZ { get => m_pressed_nz; set => SetProperty(ref m_pressed_nz, value); }

		Windows.Foundation.Point m_released_pointer;
		internal Windows.Foundation.Point NormalizedReleasedPoint { get => m_released_pointer; set => SetProperty(ref m_released_pointer, value); }
		
		double m_released_nx, m_released_ny, m_released_nz = 0;
		internal double NormalizedReleasedX { get => m_released_nx; set => SetProperty(ref m_released_nx, value); }
		internal double NormalizedReleasedY { get => m_released_ny; set => SetProperty(ref m_released_ny, value); }
		internal double NormalizedReleasedZ { get => m_released_nz; set => SetProperty(ref m_released_nz, value); }

		internal double ActualHeight { get; set; }
		internal double ActualWidth { get; set; }
		internal Vector2 SwapChainActualSize { get; set; }
	}
}
