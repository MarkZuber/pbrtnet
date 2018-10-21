// -----------------------------------------------------------------------
// <copyright file="Point2D.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace PbrtNet.Core.Geometry
{
  public class Point2D
  {
    public Point2D()
    {
      X = 0.0;
      Y = 0.0;
    }

    public Point2D(double x, double y)
    {
      X = x;
      Y = y;
    }

    public Point2D(Point2D other)
    {
      X = other.X;
      Y = other.Y;
    }

    public double X { get; }
    public double Y { get; }

    public bool HasNaNs => double.IsNaN(X) || double.IsNaN(Y);

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

    public double LengthSquared => X * X + Y * Y;
    public double Length => Math.Sqrt(LengthSquared);

    public Vector2D ToVector2D()
    {
      return new Vector2D(X, Y);
    }

    public bool IsInside(Bounds2D b)
    {
      return (X >= b.MinPoint.X && X <= b.MaxPoint.X && Y >= b.MinPoint.Y && Y <= b.MaxPoint.Y);
    }

    public double Distance(Point2D p)
    {
      return (this - p).Length;
    }

    /// <inheritdoc />
    public override string ToString()
    {
      return $"[ {X}, {Y} ]";
    }

    public static Point2D operator +(Point2D a, Point2D b)
    {
      return new Point2D(a.X + b.X, a.Y + b.Y);
    }

    public static Point2D operator -(Point2D a, Point2D b)
    {
      return new Point2D(a.X - b.X, a.Y - b.Y);
    }

    public static Point2D operator -(Point2D a)
    {
      return new Point2D(-a.X, -a.Y);
    }

    public static Point2D operator *(Point2D a, double b)
    {
      return new Point2D(a.X * b, a.Y * b);
    }

    public static Point2D operator *(double b, Point2D a)
    {
      return new Point2D(a.X * b, a.Y * b);
    }

    public static Point2D operator /(Point2D a, double b)
    {
      double inv = 1.0 / b;
      return new Point2D(a.X * inv, a.Y * inv);
    }

    public double Dot(Point2D other)
    {
      return X * other.X + Y * other.Y;
    }

    public double AbsDot(Point2D other)
    {
      return Math.Abs(Dot(other));
    }

    public Point2D Normalize()
    {
      return this / Length;
    }

    public Point2D Abs()
    {
      return new Point2D(Math.Abs(X), Math.Abs(Y));
    }

    public Point2D Floor()
    {
      return new Point2D(Math.Floor(X), Math.Floor(Y));
    }

    public Point2D Ceiling()
    {
      return new Point2D(Math.Ceiling(X), Math.Ceiling(Y));
    }

    public Point2I ToPoint2I()
    {
      return new Point2I(Convert.ToInt32(X), Convert.ToInt32(Y));
    }
  }
}