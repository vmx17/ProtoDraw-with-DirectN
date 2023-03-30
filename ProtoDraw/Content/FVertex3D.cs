using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using ABI.Windows.Foundation;

namespace DirectNXAML.Content
{
    /// <summary>
    /// 3D vertex in float
    /// </summary>
    public class FVertex3D : Primitive
    {
        public new const int Stride = 13;
        // inner struct: POD
        private struct CoreData
        {
            internal Vector3 pos;// => new(0f, 0f, fZ);       // x, y, z
            internal Vector3 nor;// => new(0f, 0f, -1f );     // NOR
            internal Vector2 tex;// => new (0, 0f );          // TEX
            internal Vector4 col;// => new(1f, 1f, 1f, 1f );  // rgba
            internal float   thick; // => new(2.0f)           // Thickness
        }
        CoreData m_c;
        public FVertex3D(float _thick = 2.0f)
        {
            m_c.pos = new(0f, 0f, Fz);      // x, y, z
            m_c.nor = new(0f, 0f, 1f);      // NOR
            m_c.tex = new(0f, 0f);          // TEX
            m_c.col = new(1f, 1f, 1f, 1f);  // rgba
            m_c.thick = _thick;             // Thickness
        }
        public FVertex3D(in FVertex3D _p, float _thick = 2.0f) : this(_thick)
        {
            m_c.pos = _p.Pos;
        }
        public FVertex3D(in Windows.Foundation.Point _p, float _r = 1.0f, float _g = 1.0f, float _b = 1.0f, float _thick = 2.0f) : this(_thick)
        {
            m_c.pos.X = (float)_p.X;
            m_c.pos.Y = (float)_p.Y;
            m_c.col.X = _r;
            m_c.col.Y = _g;
            m_c.col.Z = _b;
        }
        // at least, two float should be specified.
        public FVertex3D(float _x, float _y, float _r = 1.0f, float _g = 1.0f, float _b = 1.0f, float _thick = 2.0f) : this(_thick)
        {
            m_c.pos.X = _x;
            m_c.pos.Y = _y;
            m_c.col.X = _r;
            m_c.col.Y = _g;
            m_c.col.Z = _b;
        }
        public FVertex3D(Vector3 _pos, float _thick = 2.0f) : this(_thick)
        {
            m_c.pos = _pos;
        }
        public FVertex3D(Vector3 _pos, Vector3 _nor, float _thick = 2.0f) : this(_thick)
        {
            m_c.pos = _pos;
            m_c.nor = _nor;
        }
        public FVertex3D(Vector3 _pos, Vector4 _col, float _thick = 2.0f) : this(_thick)
        {
            m_c.pos = _pos;
            m_c.col = _col;
        }
        public FVertex3D(Vector3 _pos, Vector3 _nor, Vector2 _tex, float _thick = 2.0f) : this(_thick)
        {
            m_c.pos = _pos;
            m_c.nor = _nor;
            m_c.tex = _tex;
        }
        public FVertex3D(Vector3 _pos, Vector3 _nor, Vector2 _tex, Vector4 _col, float _thick = 2.0f) : this(_thick)
        {
            m_c.pos = _pos;
            m_c.nor = _nor;
            m_c.tex = _tex;
            m_c.col = _col;
        }
        public FVertex3D(in FVertex3D _v) : this(_v.Thick)
        {
            m_c.pos = _v.Pos;
            m_c.nor = _v.Nor;
            m_c.tex = _v.Tex;
            m_c.col = _v.Col;
        }
        public float X { get { return m_c.pos.X; } set { m_c.pos.X = value; } }
        public float Y { get { return m_c.pos.Y; } set { m_c.pos.Y = value; } }
        public float Z { get { return m_c.pos.Z; } set { m_c.pos.Z = value; } }
        public float Nx { get { return m_c.nor.X; } set { m_c.nor.X = value; } }
        public float Ny { get { return m_c.nor.Y; } set { m_c.nor.Y = value; } }
        public float Nz { get { return m_c.nor.Z; } set { m_c.nor.Z = value; } }
        public float Tx { get { return m_c.tex.X; } set { m_c.tex.X = value; } }
        public float Ty { get { return m_c.tex.Y; } set { m_c.tex.Y = value; } }
        public float R { get { return m_c.col.X; } set { m_c.col.X = value; } }
        public float G { get { return m_c.col.Y; } set { m_c.col.Y = value; } }
        public float B { get { return m_c.col.Z; } set { m_c.col.Z = value; } }
        public float A { get { return m_c.col.W; } set { m_c.col.W = value; } }
        public float Thick { get { return m_c.thick; } set { m_c.thick = value; } }
        public Vector3 Pos { get { return m_c.pos; } set { m_c.pos = value; } }
        public Vector3 Nor { get { return m_c.nor; } set { m_c.nor = value; } }
        public Vector2 Tex { get { return m_c.tex; } set { m_c.tex = value; } }
        public Vector4 Col { get { return m_c.col; } set { m_c.col = value; } }
        public void SetPos(float _x, float _y, float _z = Fz)
        {
            m_c.pos.X = _x; m_c.pos.Y = _y; m_c.pos.Z = _z;
        }
        public void SetPos(Vector3 _p)
        {
            m_c.pos = _p;
        }
        public void SetNor(float _x, float _y, float _z = -1.0f)
        {
            m_c.nor.X = _x; m_c.nor.Y = _y; m_c.nor.Z = _z;
        }
        public void SetNor(Vector3 _n)
        {
            m_c.nor = _n;
        }
        public void SetTex(float _x, float _y)
        {
            m_c.tex.X = _x; m_c.tex.Y = _y;
        }
        public void SetTex(Vector2 _t)
        {
            m_c.tex = _t;
        }
        public void SetCol(float _r, float _g, float _b, float _a = 1.0f)
        {
            m_c.col.X = _r; m_c.col.Y = _g; m_c.col.Z = _b; m_c.col.W = _a;
        }
        public override void SetCol(Vector4 _c)
        {
            m_c.col = _c;
        }
        public void SetThick(float _thick)
        {
            m_c.thick = _thick;
        }
        public override int ByteSize { get => sizeof(float) * Stride; }

        public override float[] ToFloatArray()
        {
            // here it makes float[12]
            return new float[]
            {
                m_c.pos.X, m_c.pos.Y, m_c.pos.Z,
                m_c.nor.X, m_c.nor.Y, m_c.nor.Z,
                m_c.tex.X, m_c.tex.Y,
                m_c.col.X, m_c.col.Y, m_c.col.Z, m_c.col.W,
                m_c.thick
            };
        }
        public List<float> ToList()
        {
            return this.ToFloatArray().ToList();
        }
    }
}
