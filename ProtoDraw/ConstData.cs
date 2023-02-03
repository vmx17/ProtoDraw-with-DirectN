using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectNXAML
{
    public readonly struct ColorData
    {
        public ColorData() { }
        public static readonly float4 Gray = new float4(0.5f, 0.5f, 0.5f, 1.000f);
        public static readonly float4 Blue = new float4(0.0f, 0.0f, 1.0f, 1.000f);
        public static readonly float4 White = new float4(1.0f, 1.0f, 1.0f, 1.000f);
        public static readonly float4 Black = new float4(0.0f, 0.0f, 0.0f, 1.000f);

        //for use freely
        public static float4 Line { get; set; } = Gray;
        public static void ResetLineColor() { Line = Gray; }
        public static void SetLine(float4 _color)
        {
            Line = _color;
        }
    }
}
