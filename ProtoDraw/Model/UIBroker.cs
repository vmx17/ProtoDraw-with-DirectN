using DirectNXAML.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectNXAML.Model
{
    public struct UIBroker
    {
        public IntPtr hWnd;
        public RendererBase Renderer;
        public DrawManagerBase DrawManager;

    }
}
