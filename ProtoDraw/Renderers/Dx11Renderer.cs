using DirectN;
using DirectNXAML.DrawData;
using DirectNXAML.Model;
using JeremyAnsel.DirectX.DXMath;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;        // for Path.Combine
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
// https://github.com/smourier/DirectN/issues/8


namespace DirectNXAML.Renderers
{
    public class Dx11Renderer : RendererBase
    {
        private IComObject<IDXGIDevice1> m_dxgiDevice;
        private IComObject<ID3D11Device> m_device;
        private IComObject<ID3D11DeviceContext> m_deviceContext;
        private IComObject<IDXGISwapChain1> m_swapChain;
        private IComObject<ID3D11RenderTargetView> m_renderTargetView;
        private IComObject<ID3D11DepthStencilView> m_depthStencilView;
        private D3D11_VIEWPORT m_viewPort;

        private IComObject<ID3D11Buffer> m_constantBuffer;
        private IComObject<ID3D11Buffer> m_vertexBuffer;
        private IComObject<ID3D11InputLayout> _inputLayout;
        private IComObject<ID3D11VertexShader> _vertexShader;
        private IComObject<ID3D11PixelShader> _pixelShader;
        private IComObject<ID3D11ShaderResourceView> _shaderResourceView;

        private float m_width;
        private float m_height;
        private float m_nearZ = 1000.0f;
        private float m_farZ = 1000000.0f;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_beginToStart"></param>
        public Dx11Renderer(bool _beginToStart = false)
        {
            ((App)Application.Current).DrawManager = new DrawManager();
            if (_beginToStart)
            {
                Microsoft.UI.Xaml.Media.CompositionTarget.Rendering += CompositionTarget_Rendering;
            }
        }
        public override void Dispose()
        {
            StopRendering();
            CleanUp();
        }
        public override void StartRendering()
        {
            Microsoft.UI.Xaml.Media.CompositionTarget.Rendering += CompositionTarget_Rendering;
        }
        public override void StopRendering()
        {
            Microsoft.UI.Xaml.Media.CompositionTarget.Rendering -= CompositionTarget_Rendering;
        }

        private void CompositionTarget_Rendering(object sender, object e)
        {
            if (m_swapChainPanel == null || m_swapChain == null) return;
            Render();
            m_swapChain.Object.Present(0, 0);
        }

        /// <summary>
        /// CleanUp
        /// </summary>
        public void CleanUp()
        {
            StopRendering();
            SetSwapChainPanel(null);

            if (!m_deviceContext.IsDisposed)
            {
                m_deviceContext.Object.OMSetRenderTargets(0, null, null);
                m_deviceContext.Object.ClearState();
                m_deviceContext.Dispose();
            }
            if (!m_swapChain.IsDisposed)
            {
                m_swapChain.Object.GetDevice1().Dispose();
                m_swapChain.Dispose();
            }

            if (!m_renderTargetView.IsDisposed) m_renderTargetView.Dispose();
            if (!m_constantBuffer.IsDisposed) m_constantBuffer.Dispose();
            if (!m_vertexBuffer.IsDisposed) m_vertexBuffer.Dispose();
            if (!m_depthStencilView.IsDisposed) m_depthStencilView.Dispose();
            if (!_inputLayout.IsDisposed) _inputLayout.Dispose();
            if (!_vertexShader.IsDisposed) _vertexShader.Dispose();
            if (!_pixelShader.IsDisposed) _pixelShader.Dispose();
            if ((_shaderResourceView != null) && !_shaderResourceView.IsDisposed) _shaderResourceView.Dispose();

        }

        #region Initialize
        public override void Initialize(uint _width = 1366, uint _height = 768)
        {
            lock (m_CriticalLock)
            {
                m_width = _width;
                m_height = _height;
                var fac = DXGIFunctions.CreateDXGIFactory2(DXGI_CREATE_FACTORY_FLAGS.DXGI_CREATE_FACTORY_DEBUG);
                var flags = D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_BGRA_SUPPORT | D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_DEBUG;
                m_device = D3D11Functions.D3D11CreateDevice(null, D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE, flags, out m_deviceContext);

                var desc = new DXGI_SWAP_CHAIN_DESC1();
                desc.Width = (uint)m_width;
                desc.Height = (uint)m_height;
                desc.Format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM;
                desc.Stereo = false;
                desc.SampleDesc.Count = 1;
                desc.SampleDesc.Quality = 0;
                desc.BufferUsage = Constants.DXGI_USAGE_RENDER_TARGET_OUTPUT;
                desc.BufferCount = 2;
                desc.Scaling = DXGI_SCALING.DXGI_SCALING_STRETCH;
                desc.SwapEffect = DXGI_SWAP_EFFECT.DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL;
                desc.AlphaMode = DXGI_ALPHA_MODE.DXGI_ALPHA_MODE_UNSPECIFIED;
                desc.Flags = 0;

                IDXGIDevice1 dxgiDevice = m_device.As<IDXGIDevice1>(true);
                m_dxgiDevice = new ComObject<IDXGIDevice1>(dxgiDevice);

                m_swapChain = fac.CreateSwapChainForComposition<IDXGISwapChain1>(m_dxgiDevice, desc);

                
                var frameBuffer = m_swapChain.GetBuffer<ID3D11Texture2D>(0);
                m_renderTargetView = m_device.CreateRenderTargetView(frameBuffer);

                frameBuffer.Object.GetDesc(out var depthBufferDesc);
                m_width = depthBufferDesc.Width;    // meanless
                m_height = depthBufferDesc.Height;
                m_aspectRatio = m_width / m_height;

                depthBufferDesc.Format = DXGI_FORMAT.DXGI_FORMAT_D24_UNORM_S8_UINT;
                depthBufferDesc.BindFlags = (uint)D3D11_BIND_FLAG.D3D11_BIND_DEPTH_STENCIL;
                var depthBuffer = m_device.CreateTexture2D<ID3D11Texture2D>(depthBufferDesc);

                m_depthStencilView = m_device.CreateDepthStencilView(depthBuffer);

                m_viewPort.TopLeftX = 0.0f;
                m_viewPort.TopLeftY = 0.0f;
                m_viewPort.Width = m_width;
                m_viewPort.Height = m_height;
                m_viewPort.MinDepth = 0.0f;
                m_viewPort.MaxDepth = 1.0f;

                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shaders.hlsl");
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("Shader file is not found at \"{0}\".", path);
                }
                var vsBlob = D3D11Functions.D3DCompileFromFile(path, "vs_main", "vs_5_0");
                _vertexShader = m_device.CreateVertexShader(vsBlob);

                var inputElements = new D3D11_INPUT_ELEMENT_DESC[] {
                    new D3D11_INPUT_ELEMENT_DESC{ SemanticName = "POS",     SemanticIndex = 0U, Format = DXGI_FORMAT.DXGI_FORMAT_R32G32B32_FLOAT,   InputSlot = 0U, AlignedByteOffset = 0U,                                                     InputSlotClass = D3D11_INPUT_CLASSIFICATION.D3D11_INPUT_PER_VERTEX_DATA, InstanceDataStepRate = 0U },
                    new D3D11_INPUT_ELEMENT_DESC{ SemanticName = "NOR",     SemanticIndex = 0U, Format = DXGI_FORMAT.DXGI_FORMAT_R32G32B32_FLOAT,   InputSlot = 0U, AlignedByteOffset = unchecked((uint)Constants.D3D11_APPEND_ALIGNED_ELEMENT),InputSlotClass = D3D11_INPUT_CLASSIFICATION.D3D11_INPUT_PER_VERTEX_DATA, InstanceDataStepRate = 0U },
                    new D3D11_INPUT_ELEMENT_DESC{ SemanticName = "TEX",     SemanticIndex = 0U, Format = DXGI_FORMAT.DXGI_FORMAT_R32G32_FLOAT,      InputSlot = 0U, AlignedByteOffset = unchecked((uint)Constants.D3D11_APPEND_ALIGNED_ELEMENT),InputSlotClass = D3D11_INPUT_CLASSIFICATION.D3D11_INPUT_PER_VERTEX_DATA, InstanceDataStepRate = 0U },
                    new D3D11_INPUT_ELEMENT_DESC{ SemanticName = "COL",     SemanticIndex = 0U, Format = DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT,InputSlot = 0U, AlignedByteOffset = unchecked((uint)Constants.D3D11_APPEND_ALIGNED_ELEMENT),InputSlotClass = D3D11_INPUT_CLASSIFICATION.D3D11_INPUT_PER_VERTEX_DATA, InstanceDataStepRate = 0U },
                    new D3D11_INPUT_ELEMENT_DESC{ SemanticName = "THICK",   SemanticIndex = 0U, Format = DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT,         InputSlot = 0U, AlignedByteOffset = unchecked((uint)Constants.D3D11_APPEND_ALIGNED_ELEMENT),InputSlotClass = D3D11_INPUT_CLASSIFICATION.D3D11_INPUT_PER_VERTEX_DATA, InstanceDataStepRate = 0U },
                };
                _inputLayout = m_device.CreateInputLayout(inputElements, vsBlob);

                var psBlob = D3D11Functions.D3DCompileFromFile(path, "ps_main", "ps_5_0");
                _pixelShader = m_device.CreatePixelShader(psBlob);

                /*var depthStencilDesc = new D3D11_DEPTH_STENCIL_DESC();
                depthStencilDesc.DepthEnable = true;
                depthStencilDesc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK.D3D11_DEPTH_WRITE_MASK_ALL;
                depthStencilDesc.DepthFunc = D3D11_COMPARISON_FUNC.D3D11_COMPARISON_LESS;
                _depthStencilState = _device.CreateDepthStencilState(depthStencilDesc);
                //*/

                var constantBufferDesc = new D3D11_BUFFER_DESC
                {
                    ByteWidth = (uint)Marshal.SizeOf<VS_CONSTANT_BUFFER>(),
                    Usage = D3D11_USAGE.D3D11_USAGE_DYNAMIC,
                    BindFlags = (uint)D3D11_BIND_FLAG.D3D11_BIND_CONSTANT_BUFFER,
                    CPUAccessFlags = (uint)D3D11_CPU_ACCESS_FLAG.D3D11_CPU_ACCESS_WRITE,
                    MiscFlags = 0U,
                    StructureByteStride = 0U
                };
                if ((constantBufferDesc.ByteWidth % 16) != 0)
                    throw new InvalidOperationException("Constant buffer size must be a multiple of 16.");

                m_constantBuffer = m_device.CreateBuffer(constantBufferDesc);

                var gc = GCHandle.Alloc(((App)Application.Current).DrawManager.VertexData, GCHandleType.Pinned);
                var vertexBufferDesc = new D3D11_BUFFER_DESC
                {
                    // consider to use static buffer if it short memory.
                    ByteWidth = (uint)((App)Application.Current).DrawManager.VertexData.SizeOf() + 144,
                    // 2358 = 14148 vertecies(x6) = 169776byte (x12) limit of Intel Celeron J4125
                    //vertexBufferDesc.ByteWidth = (uint)((App)Application.Current).DrawManager.VertexData.SizeOf() * 2358;
                    Usage = D3D11_USAGE.D3D11_USAGE_DYNAMIC,
                    BindFlags = (uint)D3D11_BIND_FLAG.D3D11_BIND_VERTEX_BUFFER,
                    CPUAccessFlags = (uint)D3D11_CPU_ACCESS_FLAG.D3D11_CPU_ACCESS_WRITE,
                    MiscFlags = 0,
                    StructureByteStride = 0
                };

                var subResourceData = new D3D11_SUBRESOURCE_DATA
                {
                    pSysMem = gc.AddrOfPinnedObject(),
                    SysMemPitch = 0U,
                    SysMemSlicePitch = 0U
                };
                m_vertexBuffer = m_device.CreateBuffer(vertexBufferDesc, subResourceData);
                gc.Free();

                var textureDesc = new D3D11_TEXTURE2D_DESC();
                textureDesc.Width = 20;
                textureDesc.Height = 20;
                textureDesc.MipLevels = 1;
                textureDesc.ArraySize = 1;
                textureDesc.Format = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM_SRGB;
                textureDesc.SampleDesc.Count = 1;
                textureDesc.Usage = D3D11_USAGE.D3D11_USAGE_IMMUTABLE;
                textureDesc.BindFlags = (uint)D3D11_BIND_FLAG.D3D11_BIND_SHADER_RESOURCE;

                gc = GCHandle.Alloc(((App)Application.Current).DrawManager.TextureData, GCHandleType.Pinned);
                var textureData = new D3D11_SUBRESOURCE_DATA();
                textureData.pSysMem = gc.AddrOfPinnedObject();
                textureData.SysMemPitch = 20 * 4; // 4 bytes per pixel
                gc.Free();

                var texture = m_device.CreateTexture2D<ID3D11Texture2D>(textureDesc, textureData);
                _shaderResourceView = m_device.CreateShaderResourceView(texture);
            }
        }
        #endregion

        #region SetSwapChain link to XAML
        public override void SetSwapChainPanel(SwapChainPanel panel)
        {
            if (m_swapChainPanel != null)
            {
                m_swapChainPanel.SizeChanged -= Panel_SizeChanged;
                var oldpanel = m_swapChainPanel.As<ISwapChainPanelNative>();
                oldpanel.SetSwapChain(null);
                m_swapChainPanel = null;
            }

            if (panel == null)
                return;

            var nativepanel = panel.As<ISwapChainPanelNative>();
            nativepanel.SetSwapChain(m_swapChain.Object);
            //panel.SizeChanged += Panel_SizeChanged;
            m_swapChainPanel = panel;
        }
        #endregion

        #region change panel size 
        public override void Panel_SizeChanged(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs e)
        {
            //need to resize the swap chain here.
            if (e.NewSize != e.PreviousSize)
            {
                StopRendering();
                CreateSizeDependentResources(e.NewSize);
                StartRendering();
            }
        }

        private void CreateSizeDependentResources(Windows.Foundation.Size _newSize)
        {
            lock (m_CriticalLock)
            {
                m_deviceContext.Object.OMSetRenderTargets(0, null, null);
                m_deviceContext.Object.Flush();

                m_renderTargetView.Dispose();
                m_renderTargetView = null;

                m_aspectRatio = (float)(_newSize.Width / _newSize.Height);
                m_swapChain.Object.ResizeBuffers(2, (uint)_newSize.Width, (uint)_newSize.Height, DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM, 0);

                m_deviceContext.Object.GetDevice(out var _device);
                var d3d11Device = new ComObject<ID3D11Device>(_device);

                var frameBuffer = m_swapChain.GetBuffer<ID3D11Texture2D>(0);
                m_renderTargetView = d3d11Device.CreateRenderTargetView(frameBuffer);

                frameBuffer.Object.GetDesc(out var depthBufferDesc);
                m_width = depthBufferDesc.Width;
                m_height = depthBufferDesc.Height;

                depthBufferDesc.Format = DXGI_FORMAT.DXGI_FORMAT_D24_UNORM_S8_UINT;
                depthBufferDesc.BindFlags = (uint)D3D11_BIND_FLAG.D3D11_BIND_DEPTH_STENCIL;
                var depthBuffer = d3d11Device.CreateTexture2D<ID3D11Texture2D>(depthBufferDesc);
                m_depthStencilView = d3d11Device.CreateDepthStencilView(depthBuffer);

                frameBuffer.Dispose();
                depthBuffer.Dispose();
                d3d11Device.Dispose();
            }
        }
        #endregion

        #region Rendering

        private Vector3 m_modelRotation = new(0, 0, 0);
        private Vector3 m_modelScale = new(1, 1, 1);
        private Vector3 m_modelTranslation = new(0, 0, 0);

        public override bool Render()
        {
            lock (m_CriticalLock)
            {
                // transform matrix
                // these are substantially constant
                var rotateX = D2D_MATRIX_4X4_F.RotationX(m_modelRotation.X);
                var rotateY = D2D_MATRIX_4X4_F.RotationY(m_modelRotation.Y);
                var rotateZ = D2D_MATRIX_4X4_F.RotationZ(m_modelRotation.Z);
                var scale = D2D_MATRIX_4X4_F.Scale(m_modelScale.X, m_modelScale.Y, m_modelScale.Z);
                var translate = D2D_MATRIX_4X4_F.Translation(m_modelTranslation.X, m_modelTranslation.Y, m_modelTranslation.Z);

                var transform = rotateX * rotateY * rotateZ * scale * translate;
                
                //var view = XMMatrix.LookAtLH(EyePosition, ForcusPosition, UpDirection);
                var view = XMMatrix.LookToRH(EyePosition, EyeDirection, UpDirection);

                // projection matrix
                XMMatrix orthographic = XMMatrix.OrthographicRH(m_width * m_viewScale, m_height * m_viewScale, m_nearZ, m_farZ);

                //*
                var f = view.ToArray();
                //var f = transform.ToArray();
                m_transform = new D2D_MATRIX_4X4_F(f[0], f[1], f[2], f[3], f[4], f[5], f[6], f[7],
                    f[8], f[9], f[10], f[11], f[12], f[13], f[14], f[15]);
                //*/
                //m_projection = new D2D_MATRIX_4X4_F((2 * m_nearZ) / m_width, 0, 0, 0, 0, (2 * m_nearZ) / m_height, 0, 0, 0, 0, m_farZ / (m_farZ - m_nearZ), 1, 0, 0, (m_nearZ * m_farZ) / (m_nearZ - m_farZ), 0);
                var p = orthographic.ToArray();
                m_projection = new D2D_MATRIX_4X4_F(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7],
                    p[8], p[9], p[10], p[11], p[12], p[13], p[14], p[15]);
                void mapAction(ref D3D11_MAPPED_SUBRESOURCE mapped, ref VS_CONSTANT_BUFFER buffer)
                {
                    buffer.Transform = m_transform;
                    buffer.Projection = m_projection;
                    buffer.LightVector = new XMFLOAT3(0, 0, -1);    // direction of light, not position of light
                }

                m_deviceContext.WithMap<VS_CONSTANT_BUFFER>(m_constantBuffer, 0, D3D11_MAP.D3D11_MAP_WRITE_DISCARD, mapAction);

                uint stride = (uint)FVertex3D.Stride * sizeof(float); // vertex size (13 floats: Vector3 position, Vector3 normal, Vector2 texcoord, Vector4 color, float thickness)
                uint offset = 0;

                m_deviceContext.Object.ClearRenderTargetView(m_renderTargetView.Object, m_renderBackgroundColor);
                m_deviceContext.Object.ClearDepthStencilView(m_depthStencilView.Object, (uint)D3D11_CLEAR_FLAG.D3D11_CLEAR_DEPTH, 1, 0);

                m_deviceContext.Object.IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY.D3D11_PRIMITIVE_TOPOLOGY_LINELIST);

                m_deviceContext.Object.IASetInputLayout(_inputLayout.Object);
                m_deviceContext.Object.IASetVertexBuffers(0, 1, new ID3D11Buffer[] { m_vertexBuffer.Object }, new uint[] { stride }, new uint[] { offset });
                //_deviceContext.Object.IASetIndexBuffer(_indexBuffer.Object, DXGI_FORMAT.DXGI_FORMAT_R32_UINT, 0);

                m_deviceContext.Object.VSSetShader(_vertexShader.Object, null, 0);
                m_deviceContext.Object.VSSetConstantBuffers(0, 1, new ID3D11Buffer[] { m_constantBuffer.Object });

                m_viewPort.Width = m_width;
                m_viewPort.Height = m_height;
                m_viewPort.MaxDepth = 1;
                m_deviceContext.Object.RSSetViewports(1, new D3D11_VIEWPORT[] { m_viewPort });

                m_deviceContext.Object.PSSetShader(_pixelShader.Object, null, 0);
                m_deviceContext.Object.PSSetShaderResources(0, 1, new ID3D11ShaderResourceView[] { _shaderResourceView.Object });

                m_deviceContext.Object.OMSetRenderTargets(1, new ID3D11RenderTargetView[] { m_renderTargetView.Object }, m_depthStencilView.Object);
                //_deviceContext.Object.OMSetDepthStencilState(_depthStencilState.Object, 0);

                m_deviceContext.Object.Draw((uint)((App)Application.Current).DrawManager.VertexData.Length/2, 0u);
            }
            return true;
        }
        #endregion

        #region update buffer data
        private uint m_previous_v_buffersize = 0;
        public override void UpdateVertexBuffer()
        {
            lock (m_CriticalLock)
            {
                StopRendering();
                MapVertexData();
                StartRendering();
            }
        }

        const int c_buffer_hist = 1440; // re-buffering histerisys
        const int c_buffer_hist_rev = c_buffer_hist * 2;
        private void MapVertexData()
        {
            uint new_vbuffer_size = (uint)((App)Application.Current).DrawManager.VertexData.SizeOf();
            if ((m_previous_v_buffersize < new_vbuffer_size) || ((new_vbuffer_size > c_buffer_hist_rev) && (new_vbuffer_size < (m_previous_v_buffersize - c_buffer_hist_rev))))
            {
                m_previous_v_buffersize = new_vbuffer_size + c_buffer_hist;   // to reduce remake time
                RemakeVBuffer(m_previous_v_buffersize);
            }
            var gc = GCHandle.Alloc(((App)Application.Current).DrawManager.VertexData, GCHandleType.Pinned);
            var vertexData = new D3D11_SUBRESOURCE_DATA();
            vertexData.pSysMem = gc.AddrOfPinnedObject();
            gc.Free();

            var map = m_deviceContext.Map(m_vertexBuffer, 0, D3D11_MAP.D3D11_MAP_WRITE_DISCARD);
            try
            {
                CopyMemory(map.pData, vertexData.pSysMem, (IntPtr)((App)Application.Current).DrawManager.VertexData.SizeOf());
            }
            catch (Exception ex)
            {
                var msg = ex.Message;   // for debug
            }
            finally
            {
                m_deviceContext.Unmap(m_vertexBuffer, 0);
            }
        }
        
        private void RemakeVBuffer(uint new_size)
        {
            m_vertexBuffer.Object.GetDesc(out D3D11_BUFFER_DESC vertexBufferDesc);
            var gc = GCHandle.Alloc(((App)Application.Current).DrawManager.VertexData, GCHandleType.Pinned);

            vertexBufferDesc.ByteWidth = new_size;
            var subResourceData = new D3D11_SUBRESOURCE_DATA
            {
                pSysMem = gc.AddrOfPinnedObject(),
                SysMemPitch = 0U,
                SysMemSlicePitch = 0U
            };

            try
            {
                if ((m_vertexBuffer != null) && !m_vertexBuffer.IsDisposed) m_vertexBuffer.Dispose();
                m_vertexBuffer = m_device.CreateBuffer(vertexBufferDesc, subResourceData);  // Runtime Callable Wrapper
            }
            catch (Exception ex)
            {
                var a = ex.Message; // for debug
            }
            gc.Free();
        }
        #endregion

        [StructLayout(LayoutKind.Sequential)]
        private struct VS_CONSTANT_BUFFER
        {
            public D2D_MATRIX_4X4_F Transform;
            public D2D_MATRIX_4X4_F Projection;
            public XMFLOAT3 LightVector;
            public float Padding; // to make sure it's 16-bytes aligned
        };

    }
}
