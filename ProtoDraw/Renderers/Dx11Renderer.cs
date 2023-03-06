using CommunityToolkit.WinUI.UI.Converters;
using DirectN;
using DirectNXAML.DrawData;
using DirectNXAML.Model;
using JeremyAnsel.DirectX.DXMath;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;        // for Path.Combine
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using WinRT;
using static DirectNXAML.DrawData.FVertex3DBase;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
// https://github.com/smourier/DirectN/issues/8


namespace DirectNXAML.Renderers
{
    public class Dx11Renderer : RendererBase
    {
        private const D3D_FEATURE_LEVEL m_featurelevel = D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_1;
        private const uint m_swapchaincount = 1;
        private const DXGI_FORMAT m_swapchainformat = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM;   // =87
        DXGI_FORMAT m_depthstencilformat = DXGI_FORMAT.DXGI_FORMAT_D24_UNORM_S8_UINT;
        private DXGI_SAMPLE_DESC m_sampledesc = new() { Count = 1, Quality = 0 };

        //private IComObject<IDXGIDevice1> m_dxgiDevice;

        private IComObject<ID3D11Device> m_device;
        private IComObject<ID3D11DeviceContext> m_deviceContext;
        private IComObject<IDXGISwapChain1> m_swapChain;
        private IComObject<ID3D11Texture2D> m_backBuffer;
        private IComObject<ID3D11RenderTargetView> m_renderTargetView;
        private IComObject<ID3D11Texture2D> m_depthtex;
        private IComObject<ID3D11DepthStencilView> m_depthStencilView;
        private IComObject<ID3D11DepthStencilState> m_depthStencilState;
        private IComObject<ID3D11RasterizerState> m_rasterizerState;
        private IComObject<ID3D11VertexShader> m_vertexShader;
        private IComObject<ID3D11GeometryShader> m_geometryShader;
        private IComObject<ID3D11PixelShader> m_pixelShader;
        private IComObject<ID3D11InputLayout> m_inputLayout;

        private D3D11_VIEWPORT m_viewPort;

        private IComObject<ID3D11Buffer> m_matrixBuffer;
        private IComObject<ID3D11Buffer> m_vertexBuffer;

        /*
        FVertex3DwithThickness[] InputData = new FVertex3DwithThickness[] {
            new FVertex3DwithThickness( 0.0f, 0.0f, 1.732051f,   1.0f, 0.0f, 0.0f, 1.0f,    2.0f),
            new FVertex3DwithThickness(-1.0f, 0.0f, 0.0f,        0.0f, 1.0f, 0.0f, 1.0f,    2.0f),
            new FVertex3DwithThickness( 1.0f, 0.0f, 0.0f,        0.0f, 0.0f, 1.0f, 1.0f,    2.0f)
        };
        //*/

        void CreateDeviceAndSwapChain(uint _width, uint _height)
        {
            var fac = DXGIFunctions.CreateDXGIFactory2(DXGI_CREATE_FACTORY_FLAGS.DXGI_CREATE_FACTORY_DEBUG);
            var flags = D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_BGRA_SUPPORT | D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_DEBUG;
            m_device = D3D11Functions.D3D11CreateDevice(null, D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE, flags, out m_deviceContext);

            var desc = new DXGI_SWAP_CHAIN_DESC1();
            desc.Width = _width;
            desc.Height = _height;
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
            var dxgiDevice1 = new ComObject<IDXGIDevice1>(dxgiDevice);

            m_swapChain = fac.CreateSwapChainForComposition<IDXGISwapChain1>(dxgiDevice1, desc);
        }

        void CreateRenderTargetView()
        {
            m_backBuffer = m_swapChain.GetBuffer<ID3D11Texture2D>(0);
            m_renderTargetView = m_device.CreateRenderTargetView(m_backBuffer, null);
        }

        void CreateDefaultRasterizerState()
        {
            D3D11_RASTERIZER_DESC desc = new D3D11_RASTERIZER_DESC();
            desc.FillMode = D3D11_FILL_MODE.D3D11_FILL_SOLID;
            desc.CullMode = D3D11_CULL_MODE.D3D11_CULL_BACK;
            desc.FrontCounterClockwise = true;
            desc.DepthBias = 0;
            desc.DepthBiasClamp = 0.0f;
            desc.SlopeScaledDepthBias = 0.0f;
            desc.DepthClipEnable = true;
            desc.ScissorEnable = false;
            desc.MultisampleEnable = false;
            desc.AntialiasedLineEnable = false;
            m_rasterizerState = m_device.CreateRasterizerState(desc);
        }

        void CreateDepthStencilState()
        {
            var fdesc = new D3D11_DEPTH_STENCILOP_DESC
            {
                StencilFailOp = D3D11_STENCIL_OP.D3D11_STENCIL_OP_KEEP,
                StencilDepthFailOp = D3D11_STENCIL_OP.D3D11_STENCIL_OP_KEEP,
                StencilPassOp = D3D11_STENCIL_OP.D3D11_STENCIL_OP_KEEP,
                StencilFunc = D3D11_COMPARISON_FUNC.D3D11_COMPARISON_ALWAYS
            };

            var bdesc = new D3D11_DEPTH_STENCILOP_DESC
            {
                StencilFailOp = D3D11_STENCIL_OP.D3D11_STENCIL_OP_KEEP,
                StencilDepthFailOp = D3D11_STENCIL_OP.D3D11_STENCIL_OP_KEEP,
                StencilPassOp = D3D11_STENCIL_OP.D3D11_STENCIL_OP_KEEP,
                StencilFunc = D3D11_COMPARISON_FUNC.D3D11_COMPARISON_ALWAYS
            };

            var desc = new D3D11_DEPTH_STENCIL_DESC();
            desc.DepthEnable = true;
            desc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK.D3D11_DEPTH_WRITE_MASK_ALL;
            desc.DepthFunc = D3D11_COMPARISON_FUNC.D3D11_COMPARISON_LESS;
            desc.StencilEnable = false;
            desc.StencilReadMask = (byte)0xff;  // D3D11_DEFAULT_STENCIL_READ_MASK;
            desc.StencilWriteMask = (byte)0xff; // D3D11_DEFAULT_STENCIL_WRITE_MASK
            desc.FrontFace = fdesc;
            desc.BackFace = bdesc;

            m_depthStencilState = m_device.CreateDepthStencilState(desc);
        }

        void CreateStencilBuffer(uint _width, uint _height)
        {
            var desc = new D3D11_TEXTURE2D_DESC
            {
                Width = _width,
                Height = _height,
                MipLevels = 1,
                ArraySize = 1,
                Format = DXGI_FORMAT.DXGI_FORMAT_R24G8_TYPELESS,
                SampleDesc = m_sampledesc,
                Usage = D3D11_USAGE.D3D11_USAGE_DEFAULT,
                BindFlags = (uint)(D3D11_BIND_FLAG.D3D11_BIND_DEPTH_STENCIL | D3D11_BIND_FLAG.D3D11_BIND_SHADER_RESOURCE)
            };
            m_depthtex = m_device.CreateTexture2D(desc);

            D3D11_DEPTH_STENCIL_VIEW_DESC dsvdesc = new()
            {
                Format = DXGI_FORMAT.DXGI_FORMAT_D24_UNORM_S8_UINT,
                ViewDimension = D3D11_DSV_DIMENSION.D3D11_DSV_DIMENSION_TEXTURE2D
                // uint Flags
                // D3D11_DEPTH_STENCIL_VIEW_DESC__union_0 __union_3
            };
            m_depthStencilView = m_device.CreateDepthStencilView(m_depthtex, dsvdesc);

        }

        bool CreateShaderFromhlslFiles()
        {
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Shaders\\Shaders.hlsl");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Shader file is not found at \"{0}\".", path);
            }

            var vsBlob = D3D11Functions.D3DCompileFromFile(path, "vs_main", "vs_5_0");
            m_vertexShader = m_device.CreateVertexShader(vsBlob);

            var inputElements = new D3D11_INPUT_ELEMENT_DESC[] {
                new D3D11_INPUT_ELEMENT_DESC{ SemanticName = "POSITION",    SemanticIndex = 0U, Format = DXGI_FORMAT.DXGI_FORMAT_R32G32B32_FLOAT,   InputSlot = 0U, AlignedByteOffset = 0U,                                                     InputSlotClass = D3D11_INPUT_CLASSIFICATION.D3D11_INPUT_PER_VERTEX_DATA, InstanceDataStepRate = 0U },
                new D3D11_INPUT_ELEMENT_DESC{ SemanticName = "COLOR",       SemanticIndex = 0U, Format = DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT,InputSlot = 0U, AlignedByteOffset = unchecked((uint)Constants.D3D11_APPEND_ALIGNED_ELEMENT),InputSlotClass = D3D11_INPUT_CLASSIFICATION.D3D11_INPUT_PER_VERTEX_DATA, InstanceDataStepRate = 0U },
                new D3D11_INPUT_ELEMENT_DESC{ SemanticName = "THICKNESS",   SemanticIndex = 0U, Format = DXGI_FORMAT.DXGI_FORMAT_R32_FLOAT,         InputSlot = 0U, AlignedByteOffset = unchecked((uint)Constants.D3D11_APPEND_ALIGNED_ELEMENT),InputSlotClass = D3D11_INPUT_CLASSIFICATION.D3D11_INPUT_PER_VERTEX_DATA, InstanceDataStepRate = 0U }
            };
            m_inputLayout = m_device.CreateInputLayout(inputElements, vsBlob);

            var gsBlob = D3D11Functions.D3DCompileFromFile(path, "gs_main", "gs_5_0");
            m_geometryShader = m_device.CreateGeometryShader(gsBlob);

            var psBlob = D3D11Functions.D3DCompileFromFile(path, "ps_main", "ps_5_0");
            m_pixelShader = m_device.CreatePixelShader(psBlob);

            return true;    // where's return false?
        }

        void CreateConstantBuffer(int v)
        {
            D3D11_BUFFER_DESC desc = new();
            desc.ByteWidth = (uint)v;
            desc.Usage = (uint)D3D11_USAGE.D3D11_USAGE_DEFAULT;
            desc.BindFlags = (uint)D3D11_BIND_FLAG.D3D11_BIND_CONSTANT_BUFFER;
            //desc.CPUAccessFlags=    // uint
            //    desc.MiscFlags=     // uint
            //    desc.StructureByteStride=   // uint
            m_matrixBuffer = m_device.CreateBuffer(desc);
        }


        private float m_width;
        private float m_height;
        private float m_nearZ = 1000.0f;
        private float m_farZ = 1000000.0f;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_beginToStart"></param>
        public Dx11Renderer(bool _beginToStart = false) : base()
        {
            ((App)Application.Current).DrawManager = new DrawManager();
            if (_beginToStart)
            {
                Microsoft.UI.Xaml.Media.CompositionTarget.Rendering += CompositionTarget_Rendering;
            }
        }
        public override void Dispose()
        {
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
            if ((m_swapChain != null) && !m_swapChain.IsDisposed)
            {
                m_swapChain.Object.GetDevice1().Dispose();
                m_swapChain.Dispose();
            }

            if (!m_matrixBuffer.IsDisposed) m_matrixBuffer.Dispose();
            if (!m_vertexBuffer.IsDisposed) m_vertexBuffer.Dispose();
            if (!m_inputLayout.IsDisposed) m_inputLayout.Dispose();
            if (!m_vertexShader.IsDisposed) m_vertexShader.Dispose();
            if (!m_geometryShader.IsDisposed) m_geometryShader.Dispose();
            if (!m_pixelShader.IsDisposed) m_pixelShader.Dispose();
            if ((m_renderTargetView != null) && !m_renderTargetView.IsDisposed) m_renderTargetView.Dispose();
            if ((m_depthtex != null) && !m_depthtex.IsDisposed) m_depthtex.Dispose();
            if ((m_depthStencilView != null) && !m_depthStencilView.IsDisposed) m_depthStencilView.Dispose();
            if ((m_depthStencilState != null) && !m_depthStencilState.IsDisposed) m_depthStencilState.Dispose();
            if ((m_rasterizerState != null) && !m_rasterizerState.IsDisposed) m_rasterizerState.Dispose();
            if ((m_backBuffer != null) && !m_backBuffer.IsDisposed) m_backBuffer.Dispose();
            if ((m_deviceContext != null) && !m_deviceContext.IsDisposed) m_deviceContext.Dispose();
            if ((m_device != null) && !m_device.IsDisposed) m_device.Dispose();

        }

        #region Initializer
        public override void Initialize(uint _width = 1024, uint _height = 1024)
        {
            if (_width == 0 || _height == 0) return;
            lock (m_CriticalLock)
            {
                m_width = _width;
                m_height = _height;
                try
                {
                    CreateDeviceAndSwapChain(_width, _height);
                    CreateRenderTargetView();
                    CreateDefaultRasterizerState();
                    CreateDepthStencilState();
                    CreateStencilBuffer(_width, _height);

                }
                catch
                {
                    CleanUp();
                    throw new InvalidOperationException("failed at initilizeing renderer.");
                }

                // レンダーターゲットに深度/ステンシルテクスチャを設定
                m_deviceContext.OMSetRenderTarget(m_renderTargetView, m_depthStencilView);
                // ビューポートの設定
                m_viewPort.TopLeftX = 0.0f;
                m_viewPort.TopLeftY = 0.0f;
                m_viewPort.Width = m_width;
                m_viewPort.Height = m_height;
                m_viewPort.MinDepth = 0.0f;
                m_viewPort.MaxDepth = 1.0f;
                m_deviceContext.RSSetViewport(m_viewPort);

                try
                {
                    if (!(CreateShaderFromhlslFiles()))
                    {

                    }
                }
                catch
                {
                    CleanUp();
                    throw new InvalidOperationException("failed at creating shaders.");
                }

                var constantBufferDesc = new D3D11_BUFFER_DESC();
                constantBufferDesc.ByteWidth = (uint)Marshal.SizeOf<VS_CONSTANT_BUFFER>();
                constantBufferDesc.Usage = D3D11_USAGE.D3D11_USAGE_DYNAMIC;
                constantBufferDesc.BindFlags = (uint)D3D11_BIND_FLAG.D3D11_BIND_CONSTANT_BUFFER;
                constantBufferDesc.CPUAccessFlags = (uint)D3D11_CPU_ACCESS_FLAG.D3D11_CPU_ACCESS_WRITE;
                constantBufferDesc.MiscFlags = 0U;
                constantBufferDesc.StructureByteStride = 0U;
                if ((constantBufferDesc.ByteWidth % 16) != 0)
                    throw new InvalidOperationException("Constant buffer size must be a multiple of 16.");

                m_matrixBuffer = m_device.CreateBuffer(constantBufferDesc);

                var gc = GCHandle.Alloc(((App)Application.Current).DrawManager.VertexData, GCHandleType.Pinned);
                var vertexBufferDesc = new D3D11_BUFFER_DESC();

                // consider to use static buffer if it short memory.
                vertexBufferDesc.ByteWidth = (uint)((App)Application.Current).DrawManager.VertexData.SizeOf() + 144;
                // 2358 = 14148 Vertices(x6) = 169776byte (x12) limit of Intel Celeron J4125
                //vertexBufferDesc.ByteWidth = (uint)((App)Application.Current).DrawManager.VertexData.SizeOf() * 2358;
                vertexBufferDesc.Usage = D3D11_USAGE.D3D11_USAGE_DYNAMIC;
                vertexBufferDesc.BindFlags = (uint)D3D11_BIND_FLAG.D3D11_BIND_VERTEX_BUFFER;
                vertexBufferDesc.CPUAccessFlags = (uint)D3D11_CPU_ACCESS_FLAG.D3D11_CPU_ACCESS_WRITE;
                vertexBufferDesc.MiscFlags = 0;
                vertexBufferDesc.StructureByteStride = 0;

                var subResourceData = new D3D11_SUBRESOURCE_DATA();
                subResourceData.pSysMem = gc.AddrOfPinnedObject();
                subResourceData.SysMemPitch = 0U;
                subResourceData.SysMemSlicePitch = 0U;
                m_vertexBuffer = m_device.CreateBuffer(vertexBufferDesc, subResourceData);
                gc.Free();
                /*
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
                //*/
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
                var view = XMMatrix.LookToRH(EyePosition, EyeDirection, UpDirection);

                // projection matrix
                XMMatrix orthographic = XMMatrix.OrthographicRH(m_width * m_viewScale, m_height * m_viewScale, m_nearZ, m_farZ);

                var f = view.ToArray();
                m_transform = new D2D_MATRIX_4X4_F(f[0], f[1], f[2], f[3], f[4], f[5], f[6], f[7],
                    f[8], f[9], f[10], f[11], f[12], f[13], f[14], f[15]);
                var p = orthographic.ToArray();
                m_projection = new D2D_MATRIX_4X4_F(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7],
                    p[8], p[9], p[10], p[11], p[12], p[13], p[14], p[15]);

                void mapAction(ref D3D11_MAPPED_SUBRESOURCE mapped, ref VS_CONSTANT_BUFFER buffer)
                {
                    buffer.Transform = m_transform;
                    buffer.Projection = m_projection;
                    buffer.LightVector = new XMFLOAT3(0, 0, -1);    // direction of light, not position of light
                }

                m_deviceContext.WithMap<VS_CONSTANT_BUFFER>(m_matrixBuffer, 0, D3D11_MAP.D3D11_MAP_WRITE_DISCARD, mapAction);

                // clear back buffer
                m_deviceContext.Object.ClearRenderTargetView(m_renderTargetView.Object, m_renderBackgroundColor);
                m_deviceContext.Object.ClearDepthStencilView(m_depthStencilView.Object, (uint)D3D11_CLEAR_FLAG.D3D11_CLEAR_DEPTH, 1.0f, (byte)0);

                // set topology - this may be fixed. no need to iterate
                m_deviceContext.Object.IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY.D3D11_PRIMITIVE_TOPOLOGY_LINELIST);

                // set input layout
                m_deviceContext.Object.IASetInputLayout(m_inputLayout.Object);
                uint stride = (uint)FVertex3DwithThickness.Stride * sizeof(float); // vertex size (8 floats: Vector3 position, Vector4 color, float thickness)
                uint offset = 0;
                m_deviceContext.Object.IASetVertexBuffers(0, 1, new ID3D11Buffer[] { m_vertexBuffer.Object }, new uint[] { stride }, new uint[] { offset });
                //_deviceContext.Object.IASetIndexBuffer(_indexBuffer.Object, DXGI_FORMAT.DXGI_FORMAT_R32_UINT, 0);

                m_deviceContext.Object.VSSetShader(m_vertexShader.Object, null, 0);
                m_deviceContext.Object.VSSetConstantBuffers(0, 1, new ID3D11Buffer[] { m_matrixBuffer.Object });

                m_viewPort.Width = m_width;
                m_viewPort.Height = m_height;
                m_viewPort.MaxDepth = 1;
                m_deviceContext.Object.RSSetViewports(1, new D3D11_VIEWPORT[] { m_viewPort });

                m_deviceContext.Object.PSSetShader(m_pixelShader.Object, null, 0);
                //_deviceContext.Object.PSSetShaderResources(0, 1, new ID3D11ShaderResourceView[] { _shaderResourceView.Object });

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

        private void MapVertexData()
        {
            uint new_vbuffer_size = (uint)((App)Application.Current).DrawManager.VertexData.SizeOf();
            if ((new_vbuffer_size > m_previous_v_buffersize) || (new_vbuffer_size < (m_previous_v_buffersize - 288)))
            {
                m_previous_v_buffersize = new_vbuffer_size + 144;   // +144: to reduce remake time
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
            //D3D11_BUFFER_DESC vertexBufferDesc;
            m_vertexBuffer.Object.GetDesc(out D3D11_BUFFER_DESC vertexBufferDesc);
            var gc = GCHandle.Alloc(((App)Application.Current).DrawManager.VertexData, GCHandleType.Pinned);
            
            vertexBufferDesc.ByteWidth = new_size;
            var subResourceData = new D3D11_SUBRESOURCE_DATA();
            subResourceData.pSysMem = gc.AddrOfPinnedObject();

            try
            {
                if ((m_vertexBuffer!=null) && !m_vertexBuffer.IsDisposed) m_vertexBuffer.Dispose();
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
