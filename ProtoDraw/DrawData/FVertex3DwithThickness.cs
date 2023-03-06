using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using ABI.Windows.Foundation;
using DirectN;

namespace DirectNXAML.DrawData
{
    /// <summary>
    /// 3D vertex in float for 2D line
    /// </summary>
    public class FVertex3DwithThickness : Primitive
    {
        public new const int Stride = 8;
        // inner struct: POD
        private struct LinePointData
        {
            internal Vector3 pos;// => new(0f, 0f, fZ);         // x, y, z
            internal Vector4 col;// => new(1f, 1f, 1f, 1f );    // rgba = xyzw
            internal float thickness;
        }
        LinePointData m_lp;
        public FVertex3DwithThickness(float _t = 2.0f)
        {
            m_lp.pos = new(0f, 0f, Fz);      // x, y, z
            m_lp.col = new(1f, 1f, 1f, 1f);  // rgba = xyzw
            m_lp.thickness = _t;  // rgba
        }
        public FVertex3DwithThickness(in FVertex3DwithThickness _p, float _t = 2.0f) : this(_t)
        {
            m_lp.pos = _p.Pos;
        }
        public FVertex3DwithThickness(in Windows.Foundation.Point _p, float _r = 1.0f, float _g = 1.0f, float _b = 1.0f, float _t = 2.0f) : this(_t)
        {
            m_lp.pos.X = (float)_p.X;
            m_lp.pos.Y = (float)_p.Y;
            m_lp.col.X = _r;
            m_lp.col.Y = _g;
            m_lp.col.Z = _b;
        }
        // at least, two float should be specified.
        public FVertex3DwithThickness(float _x, float _y, float _z = Fz, float _r = 1.0f, float _g = 1.0f, float _b = 1.0f, float _a = 1.0f, float _t = 2.0f)
        {
            m_lp.pos.X = _x;
            m_lp.pos.Y = _y;
            m_lp.pos.Z = _z;
            m_lp.col.X = _r;
            m_lp.col.Y = _g;
            m_lp.col.Z = _b;
            m_lp.col.W = _a;
            m_lp.thickness = _t;
        }
        public FVertex3DwithThickness(Vector3 _pos, float _t = 2.0f) : this(_t)
        {
            m_lp.pos = _pos;
        }
        public FVertex3DwithThickness(Vector3 _pos, Vector4 _col, float _t = 2.0f) : this(_pos, _t)
        {
            m_lp.col = _col;
        }
        public float X { get { return m_lp.pos.X; } set { m_lp.pos.X = value; } }
        public float Y { get { return m_lp.pos.Y; } set { m_lp.pos.Y = value; } }
        public float Z { get { return m_lp.pos.Z; } set { m_lp.pos.Z = value; } }
        public float T { get { return m_lp.thickness; } set { m_lp.thickness = value; } }
        public float R { get { return m_lp.col.X; } set { m_lp.col.X = value; } }
        public float G { get { return m_lp.col.Y; } set { m_lp.col.Y = value; } }
        public float B { get { return m_lp.col.Z; } set { m_lp.col.Z = value; } }
        public float A { get { return m_lp.col.W; } set { m_lp.col.W = value; } }
        public Vector3 Pos { get { return m_lp.pos; } set { m_lp.pos = value; } }
        public Vector4 Col { get { return m_lp.col; } set { m_lp.col = value; } }
        public void SetPos(float _x, float _y, float _z = Fz)
        {
            m_lp.pos.X = _x; m_lp.pos.Y = _y; m_lp.pos.Z = _z;
        }
        public void SetPos(Vector2 _p, float _z = Fz)
        {
            m_lp.pos.X = _p.X; m_lp.pos.Y = _p.Y; m_lp.pos.Z = _z;
        }
        public void SetPos(Vector3 _p)
        {
            m_lp.pos = _p;
        }
        public void SetCol(float _r, float _g, float _b, float _a = 1.0f)
        {
            m_lp.col.X = _r; m_lp.col.Y = _g; m_lp.col.Z = _b; m_lp.col.W = _a;
        }
        public override void SetCol(Vector4 _c)
        {
            m_lp.col = _c;
        }
        public void SetThickness(float _t)
        {
            m_lp.thickness = _t;
        }
        public override int ByteSize { get => sizeof(float) * Stride; }

        public override float[] ToFloatArray()
        {
            // here it makes float[12]
            return new float[]
            {
                m_lp.pos.X, m_lp.pos.Y, m_lp.pos.Z,
                m_lp.col.X, m_lp.col.Y, m_lp.col.Z, m_lp.col.W,
                m_lp.thickness
            };
        }
        public List<float> ToList()
        {
            return this.ToFloatArray().ToList();
        }
    }
}
