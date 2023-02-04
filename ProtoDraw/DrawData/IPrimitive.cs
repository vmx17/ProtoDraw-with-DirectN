using System.Numerics;

namespace DirectNXAML.DrawData
{
    public interface IPrimitive
    {
        public abstract void SetCol(Vector4 _col);
        public abstract float[] ToFloatArray(); // "ToArray" may be conflict...
    }
}
