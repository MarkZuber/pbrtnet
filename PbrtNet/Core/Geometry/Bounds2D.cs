using System;
using System.Collections.Generic;
using System.Text;

namespace PbrtNet.Core.Geometry
{
  public class Bounds2D
  {
    public Bounds2D()
    {
      MinPoint = new Point2D(-double.MaxValue, -double.MaxValue);
      MaxPoint = new Point2D(double.MaxValue, double.MaxValue);
    }

    public Bounds2D(Point2D p1, Point2D p2)
    {
      MinPoint = new Point2D(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
      MaxPoint = new Point2D(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));
    }

    public Point2D MinPoint { get; }
    public Point2D MaxPoint { get; }

    public Vector2D Diagonal => (MaxPoint - MinPoint).ToVector2D();

    public double Area
    {
      get
      {
        Vector2D d = Diagonal;
        return d.X * d.Y;
      }
    }

    public int MaximumExtent
    {
      get
      {
        var d = Diagonal;
        return d.X > d.Y ? 0 : 1;
      }
    }

    public Point2D this[int i]
    {
      get
      {
        switch (i)
        {
          case 0:
            return MinPoint;
          case 1:
            return MaxPoint;
          default:
            throw new IndexOutOfRangeException();
        }
      }
    }

    public Point2D Lerp(Point2D t)
    {
      return new Point2D(PbrtMath.Lerp(t.X, MinPoint.X, MaxPoint.X),
        PbrtMath.Lerp(t.Y, MinPoint.Y, MaxPoint.Y));
    }

    public Vector2D Offset(Point2D p)
    {
      Point2D o = p - MinPoint;
      double ox = o.X;
      double oy = o.Y;
      if (MaxPoint.X > MinPoint.X)
      {
        ox /= (MaxPoint.X - MinPoint.X);
      }

      if (MaxPoint.Y > MinPoint.Y)
      {
        oy /= (MaxPoint.Y - MinPoint.Y);
      }

      return new Vector2D(ox, oy);
    }

    public void BoundingSphere(out Point2D c, out double radius)
    {
      c = (MinPoint + MaxPoint) / 2.0;
      radius = c.IsInside(this) ? c.Distance(MaxPoint) : 0.0;
    }

    /// <inheritdoc />
    public override string ToString()
    {
      return $"[ {MinPoint} - {MaxPoint} ]";
    }

    public Bounds2I ToBounds2I()
    {
      return new Bounds2I(MinPoint.ToPoint2I(), MaxPoint.ToPoint2I());
    }
  }
}
