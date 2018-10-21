using System;
using System.Collections.Generic;
using System.Text;

namespace PbrtNet.Core.Geometry
{
  public class Point2I
  {
    public Point2I()
    {
      X = 0;
      Y = 0;
    }

    public Point2I(int x, int y)
    {
      X = x;
      Y = y;
    }

    public Point2I(Point2I other)
    {
      X = other.X;
      Y = other.Y;
    }

    public Vector2D ToVector2D()
    {
      return new Vector2D(X, Y);
    }

    public int X { get; private set; }
    public int Y { get; private set; }

    public bool HasNaNs => false;

    public bool IsInside(Bounds2I b)
    {
      return (X >= b.MinPoint.X && X <= b.MaxPoint.X && Y >= b.MinPoint.Y &&
              Y <= b.MaxPoint.Y);
    }

    public int Distance(Point2I p)
    {
      return (this - p).Length;
    }

    /// <inheritdoc />
    public override string ToString()
    {
      return $"[ {X}, {Y} ]";
    }

    public int this[int i]
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
          default:
            throw new IndexOutOfRangeException();
        }
      }
    }

    public static Point2I operator +(Point2I a, Point2I b)
    {
      return new Point2I(a.X + b.X, a.Y + b.Y);
    }

    public static Point2I operator -(Point2I a, Point2I b)
    {
      return new Point2I(a.X - b.X, a.Y - b.Y);
    }

    public static Point2I operator -(Point2I a)
    {
      return new Point2I(-a.X, -a.Y);
    }

    public static Point2I operator *(Point2I a, int b)
    {
      return new Point2I(a.X * b, a.Y * b);
    }

    public static Point2I operator *(int b, Point2I a)
    {
      return new Point2I(a.X * b, a.Y * b);
    }

    public static Point2I operator /(Point2I a, int b)
    {
      double inv = 1.0 / Convert.ToDouble(b);
      return new Point2I(Convert.ToInt32(Convert.ToDouble(a.X * inv)), 
                         Convert.ToInt32(Convert.ToDouble(a.Y) * inv));
    }

    public int LengthSquared => X * X + Y * Y;
    public int Length => Convert.ToInt32(Math.Sqrt(LengthSquared));

    public int Dot(Point2I other)
    {
      return X * other.X + Y * other.Y;
    }

    public int AbsDot(Point2I other)
    {
      return Math.Abs(Dot(other));
    }

    public Point2I Normalize()
    {
      return this / Length;
    }

    public Point2I Abs()
    {
      return new Point2I(Math.Abs(X), Math.Abs(Y));
    }

    public Vector2I ToVector2I()
    {
      return new Vector2I(X, Y);
    }

    public static Point2I Max(Point2I a, Point2I b)
    {
      return new Point2I(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
    }

    public static Point2I Min(Point2I a, Point2I b)
    {
      return new Point2I(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
    }

    public Point2D ToPoint2D()
    {
      return new Point2D(Convert.ToDouble(X), Convert.ToDouble(Y));
    }

    public bool InsideExclusive(Bounds2I b)
    {
      return (X >= b.MinPoint.X && X < b.MaxPoint.X && Y >= b.MinPoint.Y &&
              Y < b.MaxPoint.Y);
    }

}
}
