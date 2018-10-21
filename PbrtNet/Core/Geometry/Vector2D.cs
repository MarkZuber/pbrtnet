using System;

namespace PbrtNet.Core.Geometry
{
  public class Vector2D
  {
    public Vector2D()
    {
      X = 0.0;
      Y = 0.0;
    }

    public Vector2D(double x, double y)
    {
      X = x;
      Y = y;
    }

    public Vector2D(Vector2D other)
    {
      X = other.X;
      Y = other.Y;
    }

    public double X { get; }
    public double Y { get; }

    public bool HasNaNs => double.IsNaN(X) || double.IsNaN(Y);

    /// <inheritdoc />
    public override string ToString()
    {
      return $"[ {X}, {Y} ]";
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
        default:
          throw new IndexOutOfRangeException();
        }
      }
    }

    public static Vector2D operator +(Vector2D a, Vector2D b)
    {
      return new Vector2D(a.X + b.X, a.Y + b.Y);
    }

    public static Vector2D operator -(Vector2D a, Vector2D b)
    {
      return new Vector2D(a.X - b.X, a.Y - b.Y);
    }

    public static Vector2D operator -(Vector2D a)
    {
      return new Vector2D(-a.X, -a.Y);
    }

    public static Vector2D operator *(Vector2D a, double b)
    {
      return new Vector2D(a.X * b, a.Y * b);
    }

    public static Vector2D operator *(double b, Vector2D a)
    {
      return new Vector2D(a.X * b, a.Y * b);
    }

    public static Vector2D operator /(Vector2D a, double b)
    {
      double inv = 1.0 / b;
      return new Vector2D(a.X *inv, a.Y * inv);
    }

    public double LengthSquared => X * X + Y * Y;
    public double Length => Math.Sqrt(LengthSquared);

    public double Dot(Vector2D other)
    {
      return X * other.X + Y * other.Y;
    }

    public double AbsDot(Vector2D other)
    {
      return Math.Abs(Dot(other));
    }

    public Vector2D Normalize()
    {
      return this / Length;
    }

    public Vector2D Abs()
    {
      return new Vector2D(Math.Abs(X), Math.Abs(Y));
    }

    public Point2D ToPoint2D()
    {
      return new Point2D(X, Y);
    }
  }
}
