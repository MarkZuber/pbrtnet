// -----------------------------------------------------------------------
// <copyright file="Point3D.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace PbrtNet.Core.Geometry
{
  public class Point3D
  {
    public Point3D()
    {
      X = 0.0;
      Y = 0.0;
      Z = 0.0;
    }

    public Point3D(double x, double y, double z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    public double X { get; private set; }
    public double Y { get; private set; }
    public double Z { get; private set; }

    public double this[int i]
    {
      get
      {
        switch (i)
        {
        case 0:
          return X;
        case 1:
          return Y;
        case 2:
          return Z;
        default:
          throw new IndexOutOfRangeException();
        }
      }
      set
      {
        switch (i)
        {
        case 0:
          X = value;
          break;
        case 1:
          Y = value;
          break;
        case 2:
          Z = value;
          break;
        default:
          throw new IndexOutOfRangeException();
        }
      }
    }

    public Bounds3D ToBounds3D()
    {
      return new Bounds3D(this, this);
    }

    public Vector3D ToVector3D()
    {
      return new Vector3D(X, Y, Z);
    }

    public override string ToString()
    {
      return string.Format($"[ {X}, {Y}, {Z} ]");
    }

    public bool IsInside(Bounds3D b)
    {
      return (X >= b.MinPoint.X && X <= b.MaxPoint.X && Y >= b.MinPoint.Y && Y <= b.MaxPoint.Y && Z >= b.MinPoint.Z &&
              Z <= b.MaxPoint.Z);
    }

    public bool IsInsideExclusive(Bounds3D b)
    {
      return (X >= b.MinPoint.X && X < b.MaxPoint.X && Y >= b.MinPoint.Y && Y < b.MaxPoint.Y && Z >= b.MinPoint.Z &&
              Z < b.MaxPoint.Z);
    }

    public double DistanceSquared(Bounds3D b)
    {
      double dx = Math.Max(0.0, Math.Max(b.MinPoint.X - X, X - b.MaxPoint.X));
      double dy = Math.Max(0.0, Math.Max(b.MinPoint.Y - Y, Y - b.MaxPoint.Y));
      double dz = Math.Max(0.0, Math.Max(b.MinPoint.Z - Z, Z - b.MaxPoint.Z));
      return dx * dx + dy * dy + dz * dz;
    }

    public double Distance(Bounds3D b)
    {
      return Math.Sqrt(DistanceSquared(b));
    }

    public double Distance(Point3D p2)
    {
      return (this - p2).ToVector3D().Length();
    }

    public double DistanceSquared(Point3D p2)
    {
      return (this - p2).ToVector3D().LengthSquared();
    }

    public static Point3D Min(Point3D a, Point3D b)
    {
      return new Point3D(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
    }

    public static Point3D Max(Point3D a, Point3D b)
    {
      return new Point3D(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
    }

    public static Point3D operator -(Point3D a, Point3D b)
    {
      return new Point3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static Point3D operator +(Point3D a, Point3D b)
    {
      return new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Point3D operator *(Point3D a, double s)
    {
      return new Point3D(a.X * s, a.Y * s, a.Z * s);
    }

    public static Point3D operator *(double s, Point3D a)
    {
      return new Point3D(a.X * s, a.Y * s, a.Z * s);
    }

    public static Point3D operator /(Point3D a, double s)
    {
      return new Point3D(a.X / s, a.Y / s, a.Z / s);
    }

    public static Point3D operator -(Point3D a)
    {
      return new Point3D(-a.X, -a.Y, -a.Z);
    }

    public Point3D OffsetRayOrigin(Vector3D pError, Normal3D n, Vector3D w)
    {
      double d = n.Abs().Dot(pError);
      // We have tons of precision; for now bump up the offset a bunch just
      // to be extra sure that we start on the right side of the surface
      // (In case of any bugs in the epsilons code...)
      d *= 1024.0;

      Point3D offset = d * n.ToPoint3D();
      if (w.Dot(n) < 0.0)
      {
        offset = -offset;
      }

      Point3D po = this + offset;
      // Round offset point _po_ away from _p_
      for (int i = 0; i < 3; ++i)
      {
        if (offset[i] > 0.0)
        {
          po[i] = PbrtMath.NextFloatUp(po[i]);
        }
        else if (offset[i] < 0.0)
        {
          po[i] = PbrtMath.NextFloatDown(po[i]);
        }
      }

      return po;
    }
  }
}