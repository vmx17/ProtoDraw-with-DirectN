using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DirectNXAML.DrawData
{
    public class FVertex3DCore
    {
        private FVertex3DCoreData m_core;
        public FVertex3DCore() {; }
        public FVertex3DCore(float _f0, float _f1, float _f2, float _f3, float _f4, float _f5, float _f6)
        {
            m_core = new FVertex3DCoreData(_f0, _f1, _f2, _f3, _f4, _f5, _f6);
        }
        public FVertex3DCore(Vector3 _pos, Vector4 _col)
        {
            m_core.Pos = _pos;
            m_core.Col = _col;
        }
        public float[] ToArray { get => m_core.ToArray(); }
        public Vector3 Pos
        {
            get => m_core.Pos;
            set => m_core.Pos = value;
        }
        public Vector4 Col
        {
            get => m_core.Col;
            set => m_core.Col = value;
        }
        public FVertex3DCoreData CoreData { get => m_core; set => m_core = value; }
    }
}
