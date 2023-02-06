using DirectN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DirectNXAML.Helpers
{
    public static class MatrixVectorOperation
    {
        public static Vector4 Multiply(in D2D_MATRIX_4X4_F _mat, in Vector4 _vec)
        {
            // var a = _mat._11 * _vec.X + _mat._12 * _vec.Y + _mat._13 * _vec.Z + _mat._14 * _vec.W;
            // var b = _mat._21 * _vec.X + _mat._22 * _vec.Y + _mat._23 * _vec.Z + _mat._24 * _vec.W;
            // var c = _mat._31 * _vec.X + _mat._32 * _vec.Y + _mat._33 * _vec.Z + _mat._34 * _vec.W;
            // var d = _mat._41 * _vec.X + _mat._42 * _vec.Y + _mat._43 * _vec.Z + _mat._44 * _vec.W;
            return new Vector4(
                _mat._11 * _vec.X + _mat._12 * _vec.Y + _mat._13 * _vec.Z + _mat._14 * _vec.W,
                _mat._21 * _vec.X + _mat._22 * _vec.Y + _mat._23 * _vec.Z + _mat._24 * _vec.W,
                _mat._31 * _vec.X + _mat._32 * _vec.Y + _mat._33 * _vec.Z + _mat._34 * _vec.W,
                _mat._41 * _vec.X + _mat._42 * _vec.Y + _mat._43 * _vec.Z + _mat._44 * _vec.W
                );
        }
    }
}
