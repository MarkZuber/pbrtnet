// -----------------------------------------------------------------------
// <copyright file="Normal3D.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace PbrtNet.Core.Geometry
{
  public class Normal3D
  {
    public Normal3D()
    {
      X = 0.0;
      Y = 0.0;
      Z = 0.0;
    }

    public Normal3D(double x, double y, double z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public override string ToString()
    {
      return string.Format($"[ {X}, {Y}, {Z} ]");
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

    public double LengthSquared()
    {
      return this.Dot(this);
    }

    public double Length()
    {
      return Math.Sqrt(LengthSquared());
    }

    public Normal3D Normalize()
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

    public static Normal3D Min(Normal3D a, Normal3D b)
    {
      return new Normal3D(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
    }

    public static Normal3D Max(Normal3D a, Normal3D b)
    {
      return new Normal3D(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
    }

    public Normal3D Permute(int x, int y, int z)
    {
      return new Normal3D(this[x], this[y], this[z]);
    }

    public static Normal3D operator +(Normal3D a, Normal3D b)
    {
      return new Normal3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Normal3D operator -(Normal3D a, Normal3D b)
    {
      return new Normal3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static Normal3D operator -(Normal3D a)
    {
      return new Normal3D(-a.X, -a.Y, -a.Z);
    }

    public static Normal3D operator *(Normal3D a, double scalar)
    {
      return new Normal3D(a.X * scalar, a.Y * scalar, a.Z * scalar);
    }

    public static Normal3D operator *(double scalar, Normal3D a)
    {
      return new Normal3D(a.X * scalar, a.Y * scalar, a.Z * scalar);
    }

    public static Normal3D operator /(Normal3D a, double scalar)
    {
      return new Normal3D(a.X / scalar, a.Y / scalar, a.Z / scalar);
    }

    public Normal3D Abs()
    {
      return new Normal3D(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
    }

    public Normal3D AddScaled(Normal3D b, double scale)
    {
      return new Normal3D(X + scale * b.X, Y + scale * b.Y, Z + scale * b.Z);
    }

    public Vector3D Cross(Vector3D b)
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

    public double AbsDot(Normal3D b)
    {
      return Math.Abs(Dot(b));
    }

    public double AbsDot(Vector3D b)
    {
      return Math.Abs(Dot(b));
    }

    public Normal3D FaceForward(Vector3D v)
    {
      return (Dot(v) < 0.0) ? -this : this;
    }

    public Normal3D FaceForward(Normal3D n2)
    {
      return (Dot(n2) < 0.0) ? -this : this;
    }

    public Point3D ToPoint3D()
    {
      return new Point3D(X, Y, Z);
    }

    public Vector3D ToVector3D()
    {
      return new Vector3D(X, Y, Z);
    }
  }
}