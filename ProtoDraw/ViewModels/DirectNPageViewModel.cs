using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DirectN;
using DirectNXAML.DrawData;
using DirectNXAML.Helpers;
using DirectNXAML.Model;
using DirectNXAML.Renderers;
using JeremyAnsel.DirectX.DXMath;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Windows.Input;
using System.Drawing;

namespace DirectNXAML.ViewModels
{
    internal class DirectNPageViewModel : ObservableObject
	{
		Dx11Renderer m_renderer = null;
        // for a simple line drawing state transition (should elevate to Model layer)
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

        /// <summary>
        /// Constructor
        /// </summary>
        internal DirectNPageViewModel()
        {
            m_renderer = new Dx11Renderer();

            SetState_DrawLineCommand += SetState_DrawLine;
            SetState_SelectCommand += SetState_Select;

            ShaderPanel_SizeChangedCommand = new RelayCommand<SizeChangedEventArgs>(ShaderPanel_SizeChanged);
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
            // reset any state machine before here
			m_state = ELineGetState.none;
        }
        internal ICommand ShaderPanel_SizeChangedCommand { get; private set; }
        private void ShaderPanel_SizeChanged(SizeChangedEventArgs args)
        {
            SetLocalSizeText();
        }
        public ICommand ShaderPanel_PointerMovedCommand { get; private set; }
		private void ShaderPanel_PointerMoved(PointerRoutedEventArgs args)
        {
            SetLocalPointerText();
            SetNormalizedPointerPosition();
            args.Handled = true;

            if (m_state == ELineGetState.Pressed)
            {
                ((App)Application.Current).DrawManager.DelLastLine();
                m_lin.Ep.X = (float)NormalizedPointerPoint.X;
                m_lin.Ep.Y = (float)NormalizedPointerPoint.Y;
                ((App)Application.Current).DrawManager.Add(m_lin);
                m_renderer.UpdateVertexBuffer();
            }
        }
		internal ICommand ShaderPanel_PointerPressedCommand { get; private set; }
		private void ShaderPanel_PointerPressed(PointerRoutedEventArgs args)
        {
            SetNormalizedPointerPressed();
            args.Handled = true;

            if (m_state == ELineGetState.Begin)
            {
                ColorData.SetLine(ColorData.RubberLine);

                m_cx = ActualWidth / 2; m_cy = ActualHeight / 2;

                var v = new Vector4(new FVertex3D(NormalizedPressedPoint).Pos, 1f);
                var p = MatrixVectorOperation.Multiply(m_renderer.Transform, v);
                XMMatrix mat = XMMatrix.Identity;

                m_lin = new FLine3D();
                m_lin.Sp.X = m_lin.Ep.X = (float)NormalizedPressedPoint.X;
                m_lin.Sp.Y = m_lin.Ep.Y = (float)NormalizedPressedPoint.Y;
                m_lin.SetCol(ColorData.Line);   // blue rubber
                ((App)Application.Current).DrawManager.Add(m_lin);
                UpdateVertexCountDisplay();
                m_renderer.UpdateVertexBuffer();
                m_state = ELineGetState.Pressed;
            }
        }
        double m_cx, m_cy;  // Center of the screen

		internal ICommand ShaderPanel_PointerReleasedCommand { get; private set; }
		private void ShaderPanel_PointerReleased(PointerRoutedEventArgs args)
        {
            SetNormalizedPointerReleased();
            args.Handled = true;

            if (m_state == ELineGetState.Pressed)
            {
                ColorData.SetLine(ColorData.FixedLine);
                ((App)Application.Current).DrawManager.DelLastLine();
                m_lin.Ep.X = (float)NormalizedPointerPoint.X;
                m_lin.Ep.Y = (float)NormalizedPointerPoint.Y;
                m_lin.SetCol(ColorData.Line); // white : Rocked
                ((App)Application.Current).DrawManager.Add(m_lin);
                m_renderer.UpdateVertexBuffer();
                m_lin.Clear();
                m_state = ELineGetState.Begin;
            }
        }

        #region for display

        int m_vertex_count = 0;
        private string m_vertex_count_text = "Vertecies: ";
        internal string VertexCountText { get => m_vertex_count_text; set => SetProperty(ref m_vertex_count_text, value); }
        public int VertexCount { get => m_vertex_count; set => SetProperty(ref m_vertex_count, value); }
        private void UpdateVertexCountDisplay()
        {
            VertexCount = ((App)Application.Current).DrawManager.VertexData.Length;
            VertexCountText = "Vertecies: " + VertexCount.ToString();
        }

        string m_local_pointer_text = "Local Pointer:";
        internal string LocalPointerText { get => m_local_pointer_text; set => SetProperty(ref m_local_pointer_text, value); }
        private void SetLocalPointerText()
        {
            var sb = new StringBuilder("Local Pointer:(");
            sb.Append(m_local_point.X.ToString("F3", CultureInfo.InvariantCulture))
                .Append(", ")
                .Append(m_local_point.Y.ToString("F3", CultureInfo.InvariantCulture))
                .Append(") ");
            LocalPointerText = sb.ToString();
            sb.Clear();
        }

        string m_normalized_pointer_text= "Normalized Pointer:";
        internal string NormalizedPointerText { get => m_normalized_pointer_text; set => SetProperty(ref m_normalized_pointer_text, value); }
        private void SetNormalizedPointerPosition()
        {
            m_normalized_local_point.X = m_local_point.X / ActualWidth;
            m_normalized_local_point.Y = m_local_point.Y / ActualHeight;
            StringBuilder sb = new StringBuilder("Normalized Pointer:(");
            sb.Append(m_normalized_local_point.X.ToString("F3", CultureInfo.InvariantCulture))
                .Append(", ")
                .Append(m_normalized_local_point.Y.ToString("F3", CultureInfo.InvariantCulture))
                .Append(") ");
            NormalizedPointerText = sb.ToString();
            sb.Clear();
        }

        string m_normalized_pointer_pressed_text = "Normalized Pressed";
        internal string NormalizedPointerPressedText { get => m_normalized_pointer_pressed_text; set => SetProperty(ref m_normalized_pointer_pressed_text, value); }
        private void SetNormalizedPointerPressed()
        {
            m_normalized_pressed_point.X = m_pressed_point.X / ActualWidth;
            m_normalized_pressed_point.Y = m_pressed_point.Y / ActualHeight;
            StringBuilder sb = new StringBuilder("Normalized Pressed:(");
            sb.Append(m_normalized_pressed_point.X.ToString("F3", CultureInfo.InvariantCulture))
                .Append(", ")
                .Append(m_normalized_pressed_point.Y.ToString("F3", CultureInfo.InvariantCulture))
                .Append(") ");
            NormalizedPointerPressedText = sb.ToString();
            sb.Clear();
        }

        string m_normalized_pointer_released_text = "Normalized Released";
        internal string NormalizedPointerReleasedText { get => m_normalized_pointer_released_text; set => SetProperty(ref m_normalized_pointer_released_text, value); }
        private void SetNormalizedPointerReleased()
        {
            m_normalized_released_point.X = m_released_point.X / ActualWidth;
            m_normalized_released_point.Y = m_released_point.Y / ActualHeight;
            StringBuilder sb = new StringBuilder("Normalized Released:(");
            sb.Append(m_normalized_released_point.X.ToString("F3", CultureInfo.InvariantCulture))
                .Append(", ")
                .Append(m_normalized_released_point.Y.ToString("F3", CultureInfo.InvariantCulture))
                .Append(") ");
            NormalizedPointerReleasedText = sb.ToString();
        }

        string m_local_size_text = "Local Size:";
        internal string LocalSizeText { get => m_local_size_text; set => SetProperty(ref m_local_size_text, value); }
        private void SetLocalSizeText()
        {
            StringBuilder sb = new StringBuilder("Local Size:(");
            sb.Append(m_local_width.ToString("F3", CultureInfo.InvariantCulture))
                .Append(", ")
                .Append(m_local_height.ToString("F3", CultureInfo.InvariantCulture))
                .Append(") ");
            LocalSizeText = sb.ToString();
            sb.Clear();
        }
        #endregion

        internal double ActualHeight { get; set; }
        internal double ActualWidth { get; set; }
        internal Vector2 SwapChainActualSize { get; set; }

        double m_local_height, m_local_width;
		internal double LocalHeight { get => m_local_height; set => m_local_height = value; }
		internal double LocalWidth { get => m_local_width; set => m_local_width = value; }


		Windows.Foundation.Point m_local_point, m_normalized_local_point;
		internal Windows.Foundation.Point LocalPointerPoint { get => m_local_point; set => m_local_point = value; }
        internal Windows.Foundation.Point NormalizedPointerPoint { get => m_normalized_local_point; set =>  m_normalized_local_point = value; }


        Windows.Foundation.Point m_pressed_point, m_normalized_pressed_point;
        internal Windows.Foundation.Point PressedPoint { get => m_pressed_point; set => m_pressed_point = value; }
        internal Windows.Foundation.Point NormalizedPressedPoint { get => m_normalized_pressed_point; set => m_normalized_pressed_point = value; }
		

		Windows.Foundation.Point m_released_point, m_normalized_released_point;
        internal Windows.Foundation.Point ReleasedPoint { get => m_released_point; set => m_released_point = value; }
        internal Windows.Foundation.Point NormalizedReleasedPoint { get => m_normalized_released_point; set => m_normalized_released_point = value; }
	}
}
