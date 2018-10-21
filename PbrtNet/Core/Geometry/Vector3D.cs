// -----------------------------------------------------------------------
// <copyright file="Vector3D.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace PbrtNet.Core.Geometry
{
  public class Vector3D
  {
    public Vector3D()
    {
      X = 0.0;
      Y = 0.0;
      Z = 0.0;
    }

    public Vector3D(double x, double y, double z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public static Vector3D Zero => new Vector3D(0.0, 0.0, 0.0);
    public static Vector3D One => new Vector3D(1.0, 1.0, 1.0);
    public static Vector3D UnitX => new Vector3D(1.0, 0.0, 0.0);
    public static Vector3D UnitY => new Vector3D(0.0, 1.0, 0.0);
    public static Vector3D UnitZ => new Vector3D(0.0, 0.0, 1.0);

    public override string ToString()
    {
      return string.Format($"[ {X}, {Y}, {Z} ]");
    }

    public Normal3D ToNormal3D()
    {
      return new Normal3D(X, Y, Z);
    }

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
    }

    private static double ClampValue(double val, double min, double max)
    {
      if (val < min)
      {
        return min;
      }

      return val > max ? max : val;
    }

    public Vector3D Clamp(Vector3D min, Vector3D max)
    {
      return new Vector3D(ClampValue(X, min.X, max.X), ClampValue(Y, min.Y, max.Y), ClampValue(Z, min.Z, max.Z));
    }

    public static double CosVectors(Vector3D v1, Vector3D v2)
    {
      return v1.Dot(v2) / Math.Sqrt(v1.LengthSquared() * v2.LengthSquared());
    }

    public double LengthSquared()
    {
      return this.Dot(this);
    }

    public double Length()
    {
      return Math.Sqrt(LengthSquared());
    }

    public Vector3D Normalize()
    {
      return this / Length();
    }

    public double MinComponent()
    {
      return Math.Min(X, Math.Min(Y, Z));
    }

    public double MaxComponent()
    {
      return Math.Max(X, Math.Max(Y, Z));
    }

    public int MaxDimension()
    {
      return X > Y ? (X > Z ? 0 : 2) : (Y > Z ? 1 : 2);
    }

    public static Vector3D Min(Vector3D a, Vector3D b)
    {
      return new Vector3D(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
    }

    public static Vector3D Max(Vector3D a, Vector3D b)
    {
      return new Vector3D(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
    }

    public Vector3D Permute(int x, int y, int z)
    {
      return new Vector3D(this[x], this[y], this[z]);
    }

    public void CoordinateSystem(out Vector3D v2, out Vector3D v3)
    {
      if (Math.Abs(X) > Math.Abs(Y))
      {
        v2 = new Vector3D(-Z, 0.0, X) / Math.Sqrt(X * X + Z * Z);
      }
      else
      {
        v2 = new Vector3D(0.0, Z, -Y) / Math.Sqrt(Y * Y + Z * Z);
      }

      v3 = Cross(v2);
    }

    public Vector3D ToUnitVector()
    {
      double k = 1.0 / LengthSquared();
      return new Vector3D(X * k, Y * k, Z * k);
    }

    public static Vector3D operator +(Vector3D a, Vector3D b)
    {
      return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Vector3D operator -(Vector3D a, Vector3D b)
    {
      return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static Vector3D operator -(Vector3D a)
    {
      return new Vector3D(-a.X, -a.Y, -a.Z);
    }

    public static Vector3D operator *(Vector3D a, double scalar)
    {
      return new Vector3D(a.X * scalar, a.Y * scalar, a.Z * scalar);
    }

    public static Vector3D operator *(double scalar, Vector3D a)
    {
      return new Vector3D(a.X * scalar, a.Y * scalar, a.Z * scalar);
    }

    public static Vector3D operator /(Vector3D a, double scalar)
    {
      return new Vector3D(a.X / scalar, a.Y / scalar, a.Z / scalar);
    }

    public Vector3D Abs()
    {
      return new Vector3D(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
    }

    public Vector3D AddScaled(Vector3D b, double scale)
    {
      return new Vector3D(X + scale * b.X, Y + scale * b.Y, Z + scale * b.Z);
    }

    public Vector3D Cross(Vector3D b)
    {
      return new Vector3D(Y * b.Z - Z * b.Y, Z * b.X - X * b.Z, X * b.Y - Y * b.X);
    }

    public Vector3D Cross(Normal3D b)
    {
      return new Vector3D(Y * b.Z - Z * b.Y, Z * b.X - X * b.Z, X * b.Y - Y * b.X);
    }

    public double Dot(Normal3D b)
    {
      return X * b.X + Y * b.Y + Z * b.Z;
    }

    public double Dot(Vector3D b)
    {
      return X * b.X + Y * b.Y + Z * b.Z;
    }

    public double AbsDot(Vector3D b)
    {
      return Math.Abs(Dot(b));  
    }

    public double AbsDot(Normal3D b)
    {
      return Math.Abs(Dot(b));
    }

    public Vector3D FaceForward(Vector3D v2)
    {
      return (Dot(v2) < 0.0) ? -this : this;
    }

    public Vector3D FaceForward(Normal3D n)
    {
      return (Dot(n) < 0.0) ? -this : this;
    }

    public Point3D ToPoint3D()
    {
      return new Point3D(X, Y, Z);
    }
  }
}