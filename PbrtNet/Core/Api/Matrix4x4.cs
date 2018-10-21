// -----------------------------------------------------------------------
// <copyright file="Matrix4x4.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace PbrtNet.Core.Api
{
  public class Matrix4x4
  {
    private readonly double[,] _m = new double[4, 4];

    // Matrix4x4 Public Methods
    public Matrix4x4()
    {
      _m[0, 0] = _m[1, 1] = _m[2, 2] = _m[3, 3] = 1.0;
      _m[0, 1] = _m[0, 2] = _m[0, 3] = _m[1, 0] = 0.0;
      _m[1, 2] = _m[1, 3] = _m[2, 0] = _m[2, 1] = 0.0;
      _m[2, 3] = _m[3, 0] = _m[3, 1] = _m[3, 2] = 0.0;
    }

    public Matrix4x4(double[,] mat)
    {
      if (mat.GetLength(0) != 4 || mat.GetLength(1) != 4)
      {
        throw new ArgumentException("must be a 4x4 array");
      }

      Array.Copy(mat, _m, 16);
    }

    public double this[int i, int j] => _m[i, j];

    public Matrix4x4(
      double t00,
      double t01,
      double t02,
      double t03,
      double t10,
      double t11,
      double t12,
      double t13,
      double t20,
      double t21,
      double t22,
      double t23,
      double t30,
      double t31,
      double t32,
      double t33)
    {
      _m[0, 0] = t00;
      _m[0, 1] = t01;
      _m[0, 2] = t02;
      _m[0, 3] = t03;
      _m[1, 0] = t10;
      _m[1, 1] = t11;
      _m[1, 2] = t12;
      _m[1, 3] = t13;
      _m[2, 0] = t20;
      _m[2, 1] = t21;
      _m[2, 2] = t22;
      _m[2, 3] = t23;
      _m[3, 0] = t30;
      _m[3, 1] = t31;
      _m[3, 2] = t32;
      _m[3, 3] = t33;
    }

    //  bool operator ==(const Matrix4x4 &m2) const {
    //      for (int i = 0; i< 4; ++i)
    //          for (int j = 0; j< 4; ++j)
    //              if (m[i,j] != m2._m[i,j]) return false;
    //      return true;
    //  }
    //bool operator !=(const Matrix4x4 &m2) const {
    //      for (int i = 0; i< 4; ++i)
    //          for (int j = 0; j< 4; ++j)
    //              if (m[i,j] != m2._m[i,j]) return true;
    //      return false;
    //  }
    public Matrix4x4 Transpose()
    {
      return new Matrix4x4(
        _m[0, 0],
        _m[1, 0],
        _m[2, 0],
        _m[3, 0],
        _m[0, 1],
        _m[1, 1],
        _m[2, 1],
        _m[3, 1],
        _m[0, 2],
        _m[1, 2],
        _m[2, 2],
        _m[3, 2],
        _m[0, 3],
        _m[1, 3],
        _m[2, 3],
        _m[3, 3]);
    }

    public static Matrix4x4 Mul(Matrix4x4 m1, Matrix4x4 m2)
    {
      Matrix4x4 r = new Matrix4x4();
      for (int i = 0; i < 4; ++i)
      {
        for (int j = 0; j < 4; ++j)
        {
          r._m[i, j] = m1._m[i, 0] * m2._m[0, j] + m1._m[i, 1] * m2._m[1, j] + m1._m[i, 2] * m2._m[2, j] +
                       m1._m[i, 3] * m2._m[3, j];
        }
      }

      return r;
    }

    public Matrix4x4 Inverse()
    {
      int[] indxc = new int[4];
      int[] indxr = new int[4];
      int[] ipiv =
      {
        0,
        0,
        0,
        0
      };
      double[,] minv = new double[4, 4];

      Array.Copy(_m, minv, 4 * 4);
      for (int i = 0; i < 4; i++)
      {
        int irow = 0;
        int icol = 0;
        double big = 0.0;
        // Choose pivot
        for (int j = 0; j < 4; j++)
        {
          if (ipiv[j] != 1)
          {
            for (int k = 0; k < 4; k++)
            {
              if (ipiv[k] == 0)
              {
                if (Math.Abs(minv[j, k]) >= big)
                {
                  big = Math.Abs(minv[j, k]);
                  irow = j;
                  icol = k;
                }
              }
              else if (ipiv[k] > 1.0)
              {
                throw new InvalidOperationException("Singular matrix in MatrixInvert");
              }
            }
          }
        }

        ++ipiv[icol];
        // Swap rows _irow_ and _icol_ for pivot
        if (irow != icol)
        {
          for (int k = 0; k < 4; ++k)
          {
            PbrtMath.Swap(ref minv[irow, k], ref minv[icol, k]);
          }
        }

        indxr[i] = irow;
        indxc[i] = icol;
        if (minv[icol, icol] == 0.0)
        {
          throw new InvalidOperationException("Singular matrix in MatrixInvert");
        }

        // Set $m[icol,icol]$ to one by scaling row _icol_ appropriately
        double pivinv = 1.0 / minv[icol, icol];
        minv[icol, icol] = 1.0;
        for (int j = 0; j < 4; j++)
        {
          minv[icol, j] *= pivinv;
        }

        // Subtract this row from others to zero out their columns
        for (int j = 0; j < 4; j++)
        {
          if (j != icol)
          {
            double save = minv[j, icol];
            minv[j, icol] = 0;
            for (int k = 0; k < 4; k++)
            {
              minv[j, k] -= minv[icol, k] * save;
            }
          }
        }
      }

      // Swap columns to reflect permutation
      for (int j = 3; j >= 0; j--)
      {
        if (indxr[j] != indxc[j])
        {
          for (int k = 0; k < 4; k++)
          {
            PbrtMath.Swap(ref minv[k, indxr[j]], ref minv[k, indxc[j]]);
          }
        }
      }

      return new Matrix4x4(minv);
    }

    public static Matrix4x4 operator*(Matrix4x4 m1, Matrix4x4 m2) {
      double[,] mat = new double[4,4];
      for (int i = 0; i< 4; ++i)
      {
        for (int j = 0; j< 4; ++j)
        {
          mat[i,j] = m1[i,0] * m2[0,j] + m1[i,1] * m2[1,j] +
                      m1[i,2] * m2[2,j] + m1[i,3] * m2[3,j];
        }
      }

      return new Matrix4x4(mat);
    }

}
}