using DirectNXAML.DrawData;
using System.Collections.Generic;
using System.Linq;

namespace DirectNXAML.Model
{
    public class DrawManager : DrawManagerBase
    {
        List<float> m_vertex_list;
        public override int VertexDataByteSize { get => m_vertex_data.Length * sizeof(float); }
        public override uint[] TextureData { get => s_textureData; }

        /// <summary>
        /// This class handles line segment vertex data and index data
        /// Constructor
        /// </summary>
        public DrawManager()
        {
            // dummy data. m_vertex_data should be re-made before every reference time
            m_vertex_data = new float[] {  // Vertex3D[]: pos[3], nor[3], tex[2], col[4]
                  0.0f, -30000f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f,0.0f,    0.4f,0.4f,0.4f,0.4f,
                  0.0f,  30000f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f,0.0f,    0.4f,0.4f,0.4f,0.4f,
               -30000f,    0.0f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f,0.0f,    0.4f,0.4f,0.4f,0.4f,
                30000f,    0.0f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f,0.0f,    0.4f,0.4f,0.4f,0.4f,
               -100.0f, -100.0f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f,0.0f,    0.0f,1.0f,1.0f,1.0f,
                  0.0f,  100.0f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f,0.0f,    1.0f,0.0f,1.0f,1.0f,
                  0.0f,  100.0f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f,0.0f,    1.0f,0.0f,1.0f,1.0f,
                100.0f, -100.0f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f,0.0f,    1.0f,1.0f,0.0f,1.0f,
                100.0f, -100.0f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f,0.0f,    1.0f,1.0f,0.0f,1.0f,
               -100.0f, -100.0f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f,0.0f,    0.0f,1.0f,1.0f,1.0f,
            };
            m_vertex_list = m_vertex_data.ToList(); // only initialized
        }
        public override void AddLast(object _obj)
        {
            m_vertex_list.AddRange((_obj as FLine3D).Sp.ToList());  // cost
            m_vertex_list.AddRange((_obj as FLine3D).Ep.ToList());  // cost
            m_vertex_data = m_vertex_list.ToArray();    // cost just for reference from renderer
        }
        public override void DelLast()
        {
            m_vertex_list.RemoveRange(m_vertex_list.Count - FLine3D.Stride - 1, FLine3D.Stride);    // cost
            m_vertex_data = m_vertex_list.ToArray();    // cost
        }
    }
}
