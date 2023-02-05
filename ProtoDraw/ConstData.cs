using System.Numerics;

namespace DirectNXAML
{
    public readonly struct ColorData
    {
        public ColorData() { }
        public static readonly Vector4 Gray = new(0.5f, 0.5f, 0.5f, 1.000f);
        public static readonly Vector4 Blue = new(0.0f, 0.0f, 1.0f, 1.000f);
        public static readonly Vector4 White = new(1.0f, 1.0f, 1.0f, 1.000f);
        public static readonly Vector4 Black = new(0.0f, 0.0f, 0.0f, 1.000f);
        public static readonly Vector4 RubberLine = new(0.2f, 0.6f, 1.0f, 1.000f);
        public static readonly Vector4 FixedLine = new(0.3f, 1.0f, 0.3f, 1.000f);

        //for use freely
        public static Vector4 Line { get; set; } = Gray;
        public static void ResetLineColor() { Line = Gray; }
        public static void SetLine(Vector4 _color)
        {
            Line = _color;
        }
    }
}
