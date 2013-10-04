﻿namespace Core {
    using System.Drawing;

    using SlimDX.DXGI;
    using SlimDX.Direct3D11;

    using Device = SlimDX.Direct3D11.Device;

    public class ShadowMap : DisposableClass {
        private bool _disposed;
        private readonly int _width;
        private readonly int _height;
        private DepthStencilView _depthMapDSV;
        private readonly Viewport _viewport;
        private ShaderResourceView _depthMapSRV;
        private RenderTargetView _depthRTV;
        public ShaderResourceView DepthMapSRV {
            get { return _depthMapSRV; }
            private set { _depthMapSRV = value; }
        }

        public ShadowMap(Device device, int width, int height) {
            _width = width;
            _height = height;

            _viewport = new Viewport(0, 0, _width, _height, 0, 1.0f);

            var texDesc = new Texture2DDescription {
                Width = _width,
                Height = _height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R24G8_Typeless,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource ,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };

            var depthMap = new Texture2D(device, texDesc);

            texDesc = new Texture2DDescription {
                Width = _width,
                Height = _height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };
            var renderMap = new Texture2D(device, texDesc);
            
            var dsvDesc = new DepthStencilViewDescription {
                Flags = DepthStencilViewFlags.None,
                Format = Format.D24_UNorm_S8_UInt,
                Dimension = DepthStencilViewDimension.Texture2D,
                MipSlice = 0
            };
            _depthMapDSV = new DepthStencilView(device, depthMap, dsvDesc);

            var srvDesc = new ShaderResourceViewDescription {
                Format = Format.R24_UNorm_X8_Typeless,
                Dimension = ShaderResourceViewDimension.Texture2D,
                MipLevels = texDesc.MipLevels,
                MostDetailedMip = 0

            };
            var rtDesc = new RenderTargetViewDescription {
                Format = Format.R8G8B8A8_UNorm,
                Dimension = RenderTargetViewDimension.Texture2D,
                MipSlice = 0,
            };
            _depthRTV = new RenderTargetView(device, renderMap, rtDesc);

            DepthMapSRV = new ShaderResourceView(device, depthMap, srvDesc);

            Util.ReleaseCom(ref depthMap);
            Util.ReleaseCom(ref renderMap);

        }
        protected override void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    Util.ReleaseCom(ref _depthMapSRV);
                    Util.ReleaseCom(ref _depthMapDSV);
                    Util.ReleaseCom(ref _depthRTV);
                }
                _disposed = true;
            }
            base.Dispose(disposing);
        }
        public void BindDsvAndSetNullRenderTarget(DeviceContext dc) {
            dc.Rasterizer.SetViewports(_viewport);

            dc.OutputMerger.SetTargets(_depthMapDSV, _depthRTV);
            
            dc.ClearDepthStencilView(_depthMapDSV, DepthStencilClearFlags.Depth|DepthStencilClearFlags.Stencil, 1.0f, 0);
            dc.ClearRenderTargetView(_depthRTV, Color.Black);
        }
        
    }
}
