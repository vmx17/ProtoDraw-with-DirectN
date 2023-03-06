using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using ABI.Windows.Foundation;
using DirectN;
using JeremyAnsel.DirectX.DXMath;

namespace DirectNXAML.DrawData
{
    /// <summary>
    /// 3D vertex in float for 2D line
    /// </summary>
    public class FVertex3DBase : Primitive
    {
        
        public new const int Stride = 12;
        // inner struct: POD
        private struct LinePointData
        {
            internal Vector3 pos;// => new(0f, 0f, fZ);         // x, y, z
            internal Vector3 nor;// => new(0f, 0f, -1f );       // NOR
            internal Vector2 tex;// => new (0, 0f );            // TEX
            internal Vector4 col;// => new(1f, 1f, 1f, 1f );    // rgba
        }
        LinePointData m_lp;
        public FVertex3DBase()
        {
            m_lp.pos = new(0f, 0f, Fz);      // x, y, z
            m_lp.nor = new(0f, 0f, 1f);      // NOR
            m_lp.tex = new(0f, 0f);          // TEX
            m_lp.col = new(1f, 1f, 1f, 1f);  // rgba
        }
        public FVertex3DBase(in FVertex3D _p) : this()
        {
            m_lp.pos = _p.Pos;
        }
        public FVertex3DBase(in Windows.Foundation.Point _p, float _r = 1.0f, float _g = 1.0f, float _b = 1.0f) : this()
        {
            m_lp.pos.X = (float)_p.X;
            m_lp.pos.Y = (float)_p.Y;
            m_lp.col.X = _r;
            m_lp.col.Y = _g;
            m_lp.col.Z = _b;
        }
        // at least, two float should be specified.
        public FVertex3DBase(float _x, float _y, float _r = 1.0f, float _g = 1.0f, float _b = 1.0f) : this()
        {
            m_lp.pos.X = _x;
            m_lp.pos.Y = _y;
            m_lp.col.X = _r;
            m_lp.col.Y = _g;
            m_lp.col.Z = _b;
        }
        public FVertex3DBase(Vector3 _pos) : this()
        {
            m_lp.pos = _pos;
        }
        public FVertex3DBase(Vector3 _pos, Vector3 _nor) : this(_pos)
        {
            m_lp.nor = _nor;
        }
        public FVertex3DBase(Vector3 _pos, Vector4 _col) : this(_pos)
        {
            m_lp.col = _col;
        }
        public FVertex3DBase(Vector3 _pos, Vector3 _nor, Vector2 _tex) : this(_pos, _nor)
        {
            m_lp.tex = _tex;
        }
        public FVertex3DBase(Vector3 _pos, Vector3 _nor, Vector2 _tex, Vector4 _col) : this(_pos, _nor, _tex)
        {
            m_lp.col = _col;
        }
        public float X { get { return m_lp.pos.X; } set { m_lp.pos.X = value; } }
        public float Y { get { return m_lp.pos.Y; } set { m_lp.pos.Y = value; } }
        public float Z { get { return m_lp.pos.Z; } set { m_lp.pos.Z = value; } }
        public float Nx { get { return m_lp.nor.X; } set { m_lp.nor.X = value; } }
        public float Ny { get { return m_lp.nor.Y; } set { m_lp.nor.Y = value; } }
        public float Nz { get { return m_lp.nor.Z; } set { m_lp.nor.Z = value; } }
        public float Tx { get { return m_lp.tex.X; } set { m_lp.tex.X = value; } }
        public float Ty { get { return m_lp.tex.Y; } set { m_lp.tex.Y = value; } }
        public float R { get { return m_lp.col.X; } set { m_lp.col.X = value; } }
        public float G { get { return m_lp.col.Y; } set { m_lp.col.Y = value; } }
        public float B { get { return m_lp.col.Z; } set { m_lp.col.Z = value; } }
        public float A { get { return m_lp.col.W; } set { m_lp.col.W = value; } }
        public Vector3 Pos { get { return m_lp.pos; } set { m_lp.pos = value; } }
        public Vector3 Nor { get { return m_lp.nor; } set { m_lp.nor = value; } }
        public Vector2 Tex { get { return m_lp.tex; } set { m_lp.tex = value; } }
        public Vector4 Col { get { return m_lp.col; } set { m_lp.col = value; } }
        public void SetPos(float _x, float _y, float _z = Fz)
        {
            m_lp.pos.X = _x; m_lp.pos.Y = _y; m_lp.pos.Z = _z;
        }
        public void SetPos(Vector3 _p)
        {
            m_lp.pos = _p;
        }
        public void SetNor(float _x, float _y, float _z = -1.0f)
        {
            m_lp.nor.X = _x; m_lp.nor.Y = _y; m_lp.nor.Z = _z;
        }
        public void SetNor(Vector3 _n)
        {
            m_lp.nor = _n;
        }
        public void SetTex(float _x, float _y)
        {
            m_lp.tex.X = _x; m_lp.tex.Y = _y;
        }
        public void SetTex(Vector2 _tex)
        {
            m_lp.tex = _tex;
        }
        public void SetCol(float _r, float _g, float _b, float _a = 1.0f)
        {
            m_lp.col.X = _r; m_lp.col.Y = _g; m_lp.col.Z = _b; m_lp.col.W = _a;
        }
        public override void SetCol(Vector4 _c)
        {
            m_lp.col = _c;
        }
        public override int ByteSize { get => sizeof(float) * Stride; }

        public override float[] ToFloatArray()
        {
            // here it makes float[12]
            return new float[]
            {
                m_lp.pos.X, m_lp.pos.Y, m_lp.pos.Z,
                m_lp.nor.X, m_lp.nor.Y, m_lp.nor.Z,
                m_lp.tex.X, m_lp.tex.Y,
                m_lp.col.X, m_lp.col.Y, m_lp.col.Z, m_lp.col.W
            };
        }
        public List<float> ToList()
        {
            return this.ToFloatArray().ToList();
        }
    }
}
