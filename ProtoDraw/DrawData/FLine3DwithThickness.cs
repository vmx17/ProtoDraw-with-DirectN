using System;
using System.Numerics;

namespace DirectNXAML.DrawData
{
    // thickness is contained both vertices and this.
    public class FLine3DwithThickness : Primitive
    {
        public new static readonly int Stride = (FVertex3DwithThickness.Stride * 2) + 1; // FLine3D have two Vertices and thickness
        private FVertex3DwithThickness m_sp, m_ep;
        private float m_thickness;
        public FVertex3DwithThickness Sp { get => m_sp; set => m_sp = value; }
        public FVertex3DwithThickness Ep { get => m_ep; set => m_ep = value; }
        public float T { get => m_thickness; set { m_thickness = value; m_sp.T = value; m_ep.T = value; } }
        public FLine3DwithThickness(float _t = 1.0f)
        {
            m_sp = new FVertex3DwithThickness();
            m_ep = new FVertex3DwithThickness();
            m_thickness = _t;
        }
        public FLine3DwithThickness(FVertex3DwithThickness _sp, FVertex3DwithThickness _ep, float _t = 1.0f)
        {
            m_sp = new FVertex3DwithThickness(_sp);
            m_ep = new FVertex3DwithThickness(_ep);
            m_thickness = _t;
        }
        public FLine3DwithThickness(Vector4 _col, float _t = 1.0f) : this(_t)
        {
            this.SetCol(_col);
        }
        public FLine3DwithThickness(float _x0, float _y0, float _x1, float _y1, float _t = 1.0f) : this(_t)
        {
            m_sp.X = _x0; m_sp.Y = _y0;
            m_ep.X = _x1; m_ep.Y = _y1;
        }
        public override void SetCol(Vector4 _col)
        {
            Sp.SetCol(_col);
            Ep.SetCol(_col);
        }
        public void SetCol(float _r, float _g, float _b, float _a = 1.0f)
        {
            m_sp.SetCol(_r, _g, _b, _a);
            m_ep.SetCol(_r, _g, _b, _a);
        }
        public void SetThickness(float _t)
        {
            m_thickness = _t;
        }
        public void Clear()
        {
            m_sp = new(0f, 0f);
            m_ep = new(0f, 0f);
            m_thickness = 1.0f;
        }
        // width, texture,
        // We Should use LinkedList
        // this contains 25 float
        public override float[] ToFloatArray()
        {
            float[] arr = new float[Stride];
            Buffer.BlockCopy(m_sp.ToFloatArray(), 0, arr, 0, FVertex3DwithThickness.Stride);
            Buffer.BlockCopy(m_ep.ToFloatArray(), 0, arr, FVertex3DwithThickness.Stride, FVertex3DwithThickness.Stride);
            arr[FVertex3DwithThickness.Stride * 2] = m_thickness;
            return arr;
        }
        public override int ByteSize { get => Stride * sizeof(float); }
    }
}
