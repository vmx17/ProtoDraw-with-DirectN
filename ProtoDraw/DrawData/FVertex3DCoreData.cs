using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using DirectN;

namespace DirectNXAML.DrawData
{
    /// <summary>
    /// Data body
    /// </summary>
    public struct FVertex3DCoreData
    {
        private Vector3 pos = new Vector3(0f, 0f, 0f);      // position
        private Vector4 col = new Vector4(0f, 0f, 0f, 0f);  // color
        public FVertex3DCoreData() {; }
        public FVertex3DCoreData(Vector3 _pos, Vector4 _col) { pos = _pos; col = _col; }
        public FVertex3DCoreData(float _f0, float _f1, float _f2, float _f3, float _f4, float _f5, float _f6)
        {
            pos = new Vector3(_f0, _f1, _f2);
            col = new Vector4(_f3, _f4, _f5, _f6);
        }
        public float[] ToArray()
        {
            return new float[] { pos.X, pos.Y, pos.Z, col.X, col.Y, col.Z, col.W };
        }
        public Vector3 Pos { get => pos; set => pos = value; }
        public Vector4 Col { get => col; set => col = value; }
    }
    public class FVertices3DCoreData : IEnumerable
    {
        private FVertex3DCoreData[] m_vertices;
        public FVertices3DCoreData(in FVertex3DCoreData[] _arr)
        {
            m_vertices = new FVertex3DCoreData[_arr.Length];
            for (int i = 0; i < _arr.Length; i++)
            {
                m_vertices[i] = _arr[i];
            }
        }
        // Implementation for the GetEnumerator method.
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public FVertex3DCoreEnumerator GetEnumerator()
        {
            return new FVertex3DCoreEnumerator(m_vertices);
        }
    }

    public class FVertex3DCoreEnumerator : IEnumerator
    {
        private FVertex3DCoreData[] m_vertices;

        // Enumerators are positioned before the first element
        // until the first MoveNext() call.
        int position = -1;

        public FVertex3DCoreEnumerator(FVertex3DCoreData[] _list)
        {
            m_vertices = _list;
        }

        public bool MoveNext()
        {
            position++;
            return (position < m_vertices.Length);
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public FVertex3DCoreData Current
        {
            get
            {
                try
                {
                    return m_vertices[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
