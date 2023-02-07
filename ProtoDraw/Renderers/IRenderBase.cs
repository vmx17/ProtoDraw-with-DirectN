using JeremyAnsel.DirectX.DXMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DirectNXAML.Renderers
{
    public interface IRenderBase
    {
        public abstract void Initialize(uint _width, uint _height);
        public abstract void StartRendering();
        public abstract void StopRendering();
        public abstract bool Render();

        public XMVector EyePosition { get; set; }
        public XMVector EyeDirection { get; set; }
        public XMVector UpDirection { get; set; }
    }
}
