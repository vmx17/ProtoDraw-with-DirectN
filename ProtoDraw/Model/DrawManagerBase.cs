using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectNXAML.DrawData;

namespace DirectNXAML.Model
{
    public abstract class DrawManagerBase
    {
        // Line Segment vertex List : always output
        public virtual float[] VertexData { get => m_vertex_data; set => m_vertex_data = value; }
        protected float[] m_vertex_data;
        public virtual int VertexDataByteSize { get => m_vertex_data.Length * sizeof(float); }
        public abstract void AddLast(object _obj);
        public abstract void DelLast();
        public virtual int Length { get => m_vertex_data.Length; }
        public virtual int Count { get => m_vertex_data.Length; }
    }
}
