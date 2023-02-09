using DirectN;
using DirectNXAML.DrawData;
using DirectNXAML.Model;
using JeremyAnsel.DirectX.DXMath;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using System;
using System.IO;        // for Path.Combine
using System.Numerics;
using System.Runtime.InteropServices;
using WinRT;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
// https://github.com/smourier/DirectN/issues/8


namespace DirectNXAML.Renderers
{
    public class Dx11Renderer : RendererBase
    {
        private SwapChainPanel m_swapChainPanel = null;
        private IComObject<IDXGIDevice1> _dxgiDevice;
        private IComObject<ID3D11Device> _device;
        private IComObject<ID3D11DeviceContext> _deviceContext;
        private IComObject<IDXGISwapChain1> _swapChain;
        private IComObject<ID3D11RenderTargetView> _renderTargetView;
        private IComObject<ID3D11DepthStencilView> _depthStencilView;
        private D3D11_VIEWPORT _viewPort;

        private IComObject<ID3D11Buffer> _constantBuffer;
        private IComObject<ID3D11Buffer> _vertexBuffer;
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
            if (m_swapChainPanel == null || _swapChain == null) return;
            Render();
            _swapChain.Object.Present(0, 0);
        }

        /// <summary>
        /// CleanUp
        /// </summary>
        private void CleanUp()
        {
            StopRendering();
            SetSwapChainPanel(null);

            if (!_deviceContext.IsDisposed)
            {
                _deviceContext.Object.OMSetRenderTargets(0, null, null);
                _deviceContext.Object.ClearState();
                _deviceContext.Dispose();
            }
            if (!_swapChain.IsDisposed)
            {
                _swapChain.Object.GetDevice1().Dispose();
                _swapChain.Dispose();
            }

            if (!_renderTargetView.IsDisposed) _renderTargetView.Dispose();
            if (!_constantBuffer.IsDisposed) _constantBuffer.Dispose();
            if (!_vertexBuffer.IsDisposed) _vertexBuffer.Dispose();
            if (!_depthStencilView.IsDisposed) _depthStencilView.Dispose();
            if (!_inputLayout.IsDisposed) _inputLayout.Dispose();
            if (!_vertexShader.IsDisposed) _vertexShader.Dispose();
            if (!_pixelShader.IsDisposed) _pixelShader.Dispose();
            if ((_shaderResourceView != null) && !_shaderResourceView.IsDisposed) _shaderResourceView.Dispose();

        }

        #region Initialize
        public override void Initialize(uint _width = 1024, uint _height = 1024)
        {
            lock (m_CriticalLock)
            {
                m_width = _width;
                m_height = _height;
                var fac = DXGIFunctions.CreateDXGIFactory2(DXGI_CREATE_FACTORY_FLAGS.DXGI_CREATE_FACTORY_DEBUG);
                var flags = D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_BGRA_SUPPORT | D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_DEBUG;
                _device = D3D11Functions.D3D11CreateDevice(null, D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE, flags, out _deviceContext);

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

                IDXGIDevice1 dxgiDevice = _device.As<IDXGIDevice1>(true);
                _dxgiDevice = new ComObject<IDXGIDevice1>(dxgiDevice);

                _swapChain = fac.CreateSwapChainForComposition<IDXGISwapChain1>(_dxgiDevice, desc);

                
                var frameBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0);
                _renderTargetView = _device.CreateRenderTargetView(frameBuffer);

                frameBuffer.Object.GetDesc(out var depthBufferDesc);
                m_width = depthBufferDesc.Width;    // meanless
                m_height = depthBufferDesc.Height;
                
                depthBufferDesc.Format = DXGI_FORMAT.DXGI_FORMAT_D24_UNORM_S8_UINT;
                depthBufferDesc.BindFlags = (uint)D3D11_BIND_FLAG.D3D11_BIND_DEPTH_STENCIL;
                var depthBuffer = _device.CreateTexture2D<ID3D11Texture2D>(depthBufferDesc);

                _depthStencilView = _device.CreateDepthStencilView(depthBuffer);

                _viewPort.TopLeftX = 0.0f;
                _viewPort.TopLeftY = 0.0f;
                _viewPort.Width = m_width;
                _viewPort.Height = m_height;
                _viewPort.MinDepth = 0.0f;
                _viewPort.MaxDepth = 1.0f;

                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shaders.hlsl");
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("Shader file is not found at \"{0}\".", path);
                }
                var vsBlob = D3D11Functions.D3DCompileFromFile(path, "vs_main", "vs_5_0");
                _vertexShader = _device.CreateVertexShader(vsBlob);

                var inputElements = new D3D11_INPUT_ELEMENT_DESC[] {
                    new D3D11_INPUT_ELEMENT_DESC{ SemanticName = "POS", SemanticIndex = 0U, Format = DXGI_FORMAT.DXGI_FORMAT_R32G32B32_FLOAT,   InputSlot = 0U, AlignedByteOffset = 0U,                                                     InputSlotClass = D3D11_INPUT_CLASSIFICATION.D3D11_INPUT_PER_VERTEX_DATA, InstanceDataStepRate = 0U },
                    new D3D11_INPUT_ELEMENT_DESC{ SemanticName = "NOR", SemanticIndex = 0U, Format = DXGI_FORMAT.DXGI_FORMAT_R32G32B32_FLOAT,   InputSlot = 0U, AlignedByteOffset = unchecked((uint)Constants.D3D11_APPEND_ALIGNED_ELEMENT),InputSlotClass = D3D11_INPUT_CLASSIFICATION.D3D11_INPUT_PER_VERTEX_DATA, InstanceDataStepRate = 0U },
                    new D3D11_INPUT_ELEMENT_DESC{ SemanticName = "TEX", SemanticIndex = 0U, Format = DXGI_FORMAT.DXGI_FORMAT_R32G32_FLOAT,      InputSlot = 0U, AlignedByteOffset = unchecked((uint)Constants.D3D11_APPEND_ALIGNED_ELEMENT),InputSlotClass = D3D11_INPUT_CLASSIFICATION.D3D11_INPUT_PER_VERTEX_DATA, InstanceDataStepRate = 0U },
                    new D3D11_INPUT_ELEMENT_DESC{ SemanticName = "COL", SemanticIndex = 0U, Format = DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT,InputSlot = 0U, AlignedByteOffset = unchecked((uint)Constants.D3D11_APPEND_ALIGNED_ELEMENT),InputSlotClass = D3D11_INPUT_CLASSIFICATION.D3D11_INPUT_PER_VERTEX_DATA, InstanceDataStepRate = 0U },
                };
                _inputLayout = _device.CreateInputLayout(inputElements, vsBlob);

                var psBlob = D3D11Functions.D3DCompileFromFile(path, "ps_main", "ps_5_0");
                _pixelShader = _device.CreatePixelShader(psBlob);

                var constantBufferDesc = new D3D11_BUFFER_DESC();
                constantBufferDesc.ByteWidth = (uint)Marshal.SizeOf<VS_CONSTANT_BUFFER>();
                constantBufferDesc.Usage = D3D11_USAGE.D3D11_USAGE_DYNAMIC;
                constantBufferDesc.BindFlags = (uint)D3D11_BIND_FLAG.D3D11_BIND_CONSTANT_BUFFER;
                constantBufferDesc.CPUAccessFlags = (uint)D3D11_CPU_ACCESS_FLAG.D3D11_CPU_ACCESS_WRITE;
                constantBufferDesc.MiscFlags = 0U;
                constantBufferDesc.StructureByteStride = 0U;
                if ((constantBufferDesc.ByteWidth % 16) != 0)
                    throw new InvalidOperationException("Constant buffer size must be a multiple of 16.");

                _constantBuffer = _device.CreateBuffer(constantBufferDesc);

                var gc = GCHandle.Alloc(((App)Application.Current).DrawManager.VertexData, GCHandleType.Pinned);
                var vertexBufferDesc = new D3D11_BUFFER_DESC();

                // consider to use static buffer if it short memory.
                vertexBufferDesc.ByteWidth = (uint)((App)Application.Current).DrawManager.VertexData.SizeOf() + 144;
                vertexBufferDesc.Usage = D3D11_USAGE.D3D11_USAGE_DYNAMIC;
                vertexBufferDesc.BindFlags = (uint)D3D11_BIND_FLAG.D3D11_BIND_VERTEX_BUFFER;
                vertexBufferDesc.CPUAccessFlags = (uint)D3D11_CPU_ACCESS_FLAG.D3D11_CPU_ACCESS_WRITE;
                vertexBufferDesc.MiscFlags = 0;
                vertexBufferDesc.StructureByteStride = 0;

                var subResourceData = new D3D11_SUBRESOURCE_DATA();
                subResourceData.pSysMem = gc.AddrOfPinnedObject();
                subResourceData.SysMemPitch = 0U;
                subResourceData.SysMemSlicePitch = 0U;
                _vertexBuffer = _device.CreateBuffer(vertexBufferDesc, subResourceData);
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

                var texture = _device.CreateTexture2D<ID3D11Texture2D>(textureDesc, textureData);
                _shaderResourceView = _device.CreateShaderResourceView(texture);
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
            nativepanel.SetSwapChain(_swapChain.Object);
            
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
                _deviceContext.Object.OMSetRenderTargets(0, null, null);
                _deviceContext.Object.Flush();

                _renderTargetView.Dispose();
                _renderTargetView = null;

                _swapChain.Object.ResizeBuffers(2, (uint)_newSize.Width, (uint)_newSize.Height, DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM, 0);

                _deviceContext.Object.GetDevice(out var _device);
                var d3d11Device = new ComObject<ID3D11Device>(_device);

                var frameBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0);
                _renderTargetView = d3d11Device.CreateRenderTargetView(frameBuffer);

                frameBuffer.Object.GetDesc(out var depthBufferDesc);
                m_width = depthBufferDesc.Width;
                m_height = depthBufferDesc.Height;

                depthBufferDesc.Format = DXGI_FORMAT.DXGI_FORMAT_D24_UNORM_S8_UINT;
                depthBufferDesc.BindFlags = (uint)D3D11_BIND_FLAG.D3D11_BIND_DEPTH_STENCIL;
                var depthBuffer = d3d11Device.CreateTexture2D<ID3D11Texture2D>(depthBufferDesc);
                _depthStencilView = d3d11Device.CreateDepthStencilView(depthBuffer);

                frameBuffer.Dispose();
                depthBuffer.Dispose();
                d3d11Device.Dispose();
            }
        }
        #endregion

        #region Rendering

        private Vector3 m_modelRotation = new(0, 0, 0);
        private Vector3 m_modelScale = new(1, 1, 1);
        private Vector3 m_modelTranslation = new(0, 0, 1500);

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
                m_transform = rotateX * rotateY * rotateZ * scale * translate;

                // projection matrix
                XMMatrix orthographic = XMMatrix.OrthographicLH(m_width, m_height, m_nearZ, m_farZ);
                var p = orthographic.ToArray();
                m_projection = new D2D_MATRIX_4X4_F(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7],
                    p[8], p[9], p[10], p[11], p[12], p[13], p[14], p[15]);

                void mapAction(ref D3D11_MAPPED_SUBRESOURCE mapped, ref VS_CONSTANT_BUFFER buffer)
                {
                    buffer.Transform = m_transform;
                    buffer.Projection = m_projection;
                    buffer.LightVector = new XMFLOAT3(0, 0, 500);
                }

                _deviceContext.WithMap<VS_CONSTANT_BUFFER>(_constantBuffer, 0, D3D11_MAP.D3D11_MAP_WRITE_DISCARD, mapAction);

                uint stride = (uint)FVertex3D.Stride * sizeof(float); // vertex size (12 floats: Vector3 position, Vector3 normal, Vector2 texcoord, Vector4 color)
                uint offset = 0;

                _deviceContext.Object.ClearRenderTargetView(_renderTargetView.Object, m_renderBackgroundColor);
                _deviceContext.Object.ClearDepthStencilView(_depthStencilView.Object, (uint)D3D11_CLEAR_FLAG.D3D11_CLEAR_DEPTH, 1, 0);
                _deviceContext.Object.IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY.D3D11_PRIMITIVE_TOPOLOGY_LINELIST);

                _deviceContext.Object.IASetInputLayout(_inputLayout.Object);
                _deviceContext.Object.IASetVertexBuffers(0, 1, new ID3D11Buffer[] { _vertexBuffer.Object }, new uint[] { stride }, new uint[] { offset });

                _deviceContext.Object.VSSetShader(_vertexShader.Object, null, 0);
                _deviceContext.Object.VSSetConstantBuffers(0, 1, new ID3D11Buffer[] { _constantBuffer.Object });

                _viewPort.Width = m_width;
                _viewPort.Height = m_height;
                _viewPort.MaxDepth = 1;
                _deviceContext.Object.RSSetViewports(1, new D3D11_VIEWPORT[] { _viewPort });

                _deviceContext.Object.PSSetShader(_pixelShader.Object, null, 0);
                _deviceContext.Object.PSSetShaderResources(0, 1, new ID3D11ShaderResourceView[] { _shaderResourceView.Object });

                _deviceContext.Object.OMSetRenderTargets(1, new ID3D11RenderTargetView[] { _renderTargetView.Object }, _depthStencilView.Object);

                _deviceContext.Object.Draw((uint)((App)Application.Current).DrawManager.VertexData.Length/2, 0u);
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

        private void MapVertexData()
        {
            uint new_vbuffer_size = (uint)((App)Application.Current).DrawManager.VertexData.SizeOf();
            if (new_vbuffer_size != m_previous_v_buffersize)
            {
                m_previous_v_buffersize = new_vbuffer_size;   // +144: to reduce remake time
                RemakeVBuffer(m_previous_v_buffersize);
            }
            var gc = GCHandle.Alloc(((App)Application.Current).DrawManager.VertexData, GCHandleType.Pinned);
            var vertexData = new D3D11_SUBRESOURCE_DATA();
            vertexData.pSysMem = gc.AddrOfPinnedObject();
            gc.Free();

            var map = _deviceContext.Map(_vertexBuffer, 0, D3D11_MAP.D3D11_MAP_WRITE_DISCARD);
            CopyMemory(map.pData, vertexData.pSysMem, (IntPtr)((App)Application.Current).DrawManager.VertexData.SizeOf());
            _deviceContext.Unmap(_vertexBuffer, 0);
        }

        private void RemakeVBuffer(uint new_size)
        {
            _vertexBuffer.Object.GetDesc(out D3D11_BUFFER_DESC vertexBufferDesc);
            var gc = GCHandle.Alloc(((App)Application.Current).DrawManager.VertexData, GCHandleType.Pinned);

            vertexBufferDesc.ByteWidth = new_size;
            var subResourceData = new D3D11_SUBRESOURCE_DATA();
            subResourceData.pSysMem = gc.AddrOfPinnedObject();

            if ((_vertexBuffer != null) && !_vertexBuffer.IsDisposed) _vertexBuffer.Dispose();
            _vertexBuffer = _device.CreateBuffer(vertexBufferDesc, subResourceData);  // Runtime Callable Wrapper

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
