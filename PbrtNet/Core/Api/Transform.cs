// -----------------------------------------------------------------------
// <copyright file="Transform.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Api
{
  public class Transform
  {
    public Transform(Matrix4x4 m)
    {
      M = m;
      MInv = m.Inverse();
    }

    public Transform(Matrix4x4 m, Matrix4x4 mInv)
    {
      M = m;
      MInv = mInv;
    }

    public static Transform Scale(double x, double y, double z)
    {
      Matrix4x4 m = new Matrix4x4(x, 0.0, 0.0, 0.0, 0.0, y, 0.0, 0.0, 0.0, 0.0, z, 0.0, 0.0, 0.0, 0.0, 1.0);
      Matrix4x4 minv = new Matrix4x4(1.0 / x, 0.0, 0.0, 0.0, 0.0, 1.0 / y, 0.0, 0.0, 0.0, 0.0, 1.0 / z, 0.0, 0.0, 0.0, 0.0, 1.0);
      return new Transform(m, minv);
    }

    public static Transform Translate(Vector3D delta) {
      Matrix4x4 m = new Matrix4x4(1.0, 0.0, 0.0, delta.X, 0.0, 1.0, 0.0, delta.Y, 0.0, 0.0, 1.0, delta.Z, 0.0, 0.0, 0.0,
                  1.0);
      Matrix4x4 minv = new Matrix4x4(1.0, 0.0, 0.0, -delta.X, 0.0, 1.0, 0.0, -delta.Y, 0.0, 0.0, 1.0, -delta.Z, 0.0,
                     0.0, 0.0, 1.0);
      return new Transform(m, minv);
    }


  public Matrix4x4 M { get; }
    public Matrix4x4 MInv { get; }

    public bool SwapsHandedness
    {
      get
      {
        double det = M[0, 0] * (M[1, 1] * M[2, 2] - M[1, 2] * M[2, 1]) -
                     M[0, 1] * (M[1, 0] * M[2, 2] - M[1, 2] * M[2, 0]) +
                     M[0, 2] * (M[1, 0] * M[2, 1] - M[1, 1] * M[2, 0]);
        return det < 0.0;
      }
    }

    public Transform Inverse()
    {
      return new Transform(MInv, M);
    }

    public bool HasScale()
    {
      double la2 = AtVector3D(new Vector3D(1.0, 0.0, 0.0)).LengthSquared();
      double lb2 = AtVector3D(new Vector3D(0.0, 1.0, 0.0)).LengthSquared();
      double lc2 = AtVector3D(new Vector3D(0.0, 0.0, 1.0)).LengthSquared();

      bool NotOne(double d)
      {
        return (d < 0.999 || d > 1.001);
      }

      return NotOne(la2) || NotOne(lb2) || NotOne(lc2);
    }

    public Vector3D AtVector3D(Vector3D v)
    {
      double x = v.X,
             y = v.Y,
             z = v.Z;
      return new Vector3D(
        M[0, 0] * x + M[0, 1] * y + M[0, 2] * z,
        M[1, 0] * x + M[1, 1] * y + M[1, 2] * z,
        M[2, 0] * x + M[2, 1] * y + M[2, 2] * z);
    }

    // todo: need to figure out better naming
    // this is the replacement for operator()(bounds3f) in the c++ code
    public Bounds3D AtBounds(Bounds3D b)
    {
      Bounds3D ret = new Bounds3D(AtPoint(new Point3D(b.MinPoint.X, b.MinPoint.Y, b.MinPoint.Z)));
      ret = ret.Union(AtPoint(new Point3D(b.MaxPoint.X, b.MinPoint.Y, b.MinPoint.Z)));
      ret = ret.Union(AtPoint(new Point3D(b.MinPoint.X, b.MaxPoint.Y, b.MinPoint.Z)));
      ret = ret.Union(AtPoint(new Point3D(b.MinPoint.X, b.MinPoint.Y, b.MaxPoint.Z)));
      ret = ret.Union(AtPoint(new Point3D(b.MinPoint.X, b.MaxPoint.Y, b.MaxPoint.Z)));
      ret = ret.Union(AtPoint(new Point3D(b.MaxPoint.X, b.MaxPoint.Y, b.MinPoint.Z)));
      ret = ret.Union(AtPoint(new Point3D(b.MaxPoint.X, b.MinPoint.Y, b.MaxPoint.Z)));
      ret = ret.Union(AtPoint(new Point3D(b.MaxPoint.X, b.MaxPoint.Y, b.MaxPoint.Z)));
      return ret;
    }

    public Point3D AtPoint(Point3D p)
    {
      double x = p.X,
             y = p.Y,
             z = p.Z;
      double xp = M[0, 0] * x + M[0, 1] * y + M[0, 2] * z + M[0, 3];
      double yp = M[1, 0] * x + M[1, 1] * y + M[1, 2] * z + M[1, 3];
      double zp = M[2, 0] * x + M[2, 1] * y + M[2, 2] * z + M[2, 3];
      double wp = M[3, 0] * x + M[3, 1] * y + M[3, 2] * z + M[3, 3];
      // todo: CHECK_NE(wp, 0);
      if (wp == 1.0)
      {
        return new Point3D(xp, yp, zp);
      }
      else
      {
        return new Point3D(xp, yp, zp) / wp;
      }
    }

    public static Transform operator*(Transform t1, Transform t2)
    {
      return new Transform(t1.M * t2.M, t1.MInv * t2.MInv);
    }

    public static Transform Perspective(double fov, float n, double f)
    {
      // Perform projective divide for perspective projection
      Matrix4x4 persp = new Matrix4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, f / (f - n), -f * n / (f - n),
                      0, 0, 1, 0);

      // Scale canonical perspective view to specified field of view
      double invTanAng = 1.0 / Math.Tan(PbrtMath.Radians(fov) / 2);
      return Transform.Scale(invTanAng, invTanAng, 1.0) * new Transform(persp);
    }

    public Ray ExecuteTransform(Ray r)
    {
      Point3D o = ExecuteTransform(r.Origin, out Vector3D oError);
      Vector3D d = ExecuteTransform(r.Direction);
      // Offset ray origin to edge of error bounds and compute _tMax_
      double lengthSquared = d.LengthSquared();
      double tMax = r.TMax;
      if (lengthSquared > 0)
      {
        double dt = d.Abs().Dot(oError) / lengthSquared;
        o += d.ToPoint3D() * dt;
        tMax -= dt;
      }
      return new Ray(o, d, tMax, r.Time, r.Medium);
    }

    public Vector3D ExecuteTransform(Vector3D v)
    {
      double x = v.X, y = v.Y, z = v.Z;
      return new Vector3D(M[0,0] * x + M[0,1] * y + M[0,2] * z,
                        M[1,0] * x + M[1,1] * y + M[1,2] * z,
                        M[2,0] * x + M[2,1] * y + M[2,2] * z);
    }

    public Point3D ExecuteTransform(Point3D p)
    {
      double x = p.X, y = p.Y, z = p.Z;
      double xp = M[0,0] * x + M[0,1] * y + M[0,2] * z + M[0,3];
      double yp = M[1,0] * x + M[1,1] * y + M[1,2] * z + M[1,3];
      double zp = M[2,0] * x + M[2,1] * y + M[2,2] * z + M[2,3];
      double wp = M[3,0] * x + M[3,1] * y + M[3,2] * z + M[3,3];
      //CHECK_NE(wp, 0);
      if (wp == 1)
      {
        return new Point3D(xp, yp, zp);
      }
      else
      {
        return new Point3D(xp, yp, zp) / wp;
      }
    }

    public Point3D ExecuteTransform(Point3D p, out Vector3D pError)
    {
      double x = p.X, y = p.Y, z = p.Z;
      // Compute transformed coordinates from point _pt_
      double xp = M[0,0] * x + M[0,1] * y + M[0,2] * z + M[0,3];
      double yp = M[1,0] * x + M[1,1] * y + M[1,2] * z + M[1,3];
      double zp = M[2,0] * x + M[2,1] * y + M[2,2] * z + M[2,3];
      double wp = M[3,0] * x + M[3,1] * y + M[3,2] * z + M[3,3];

      // Compute absolute error for transformed point
      double xAbsSum = (Math.Abs(M[0,0] * x) + Math.Abs(M[0,1] * y) +
                   Math.Abs(M[0,2] * z) + Math.Abs(M[0,3]));
      double yAbsSum = (Math.Abs(M[1,0] * x) + Math.Abs(M[1,1] * y) +
                   Math.Abs(M[1,2] * z) + Math.Abs(M[1,3]));
      double zAbsSum = (Math.Abs(M[2,0] * x) + Math.Abs(M[2,1] * y) +
                   Math.Abs(M[2,2] * z) + Math.Abs(M[2,3]));
      pError = PbrtMath.Gamma(3) * new Vector3D(xAbsSum, yAbsSum, zAbsSum);
      //CHECK_NE(wp, 0);
      if (wp == 1.0)
      {
        return new Point3D(xp, yp, zp);
      }
      else
      {
        return new Point3D(xp, yp, zp) / wp;
      }
    }
  }
}