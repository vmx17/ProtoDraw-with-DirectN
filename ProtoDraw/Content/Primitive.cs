using System.Numerics;
namespace DirectNXAML.Content
{
    public class Primitive : IPrimitive
    {
        public const int Stride = 0;
        public const float Fz = 0.0f; // Z value for 2D drawing?
        public Primitive() { }
        public virtual void SetCol(Vector4 _col) { }
        public virtual float[] ToFloatArray() { return null; }
        public virtual int ByteSize { get; private set; }
    }
}
