using System;
using System.Collections.Generic;
using System.Text;

namespace PbrtNet.Core.Geometry
{
  public class Bounds2I
  {
    public Bounds2I()
    {
      MinPoint = new Point2I(-int.MaxValue, -int.MaxValue);
      MaxPoint = new Point2I(int.MaxValue, int.MaxValue);
    }

    public Bounds2I(Point2I p1, Point2I p2)
    {
      MinPoint = new Point2I(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
      MaxPoint = new Point2I(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));
    }

    public Point2I MinPoint { get; }
    public Point2I MaxPoint { get; }

    public Vector2I Diagonal => (MaxPoint - MinPoint).ToVector2I();

    public int Area
    {
      get
      {
        Vector2I d = Diagonal;
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

    public Point2I this[int i]
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

    public Point2I Lerp(Point2I t)
    {
      return new Point2I(PbrtMath.Lerp(t.X, MinPoint.X, MaxPoint.X),
                         PbrtMath.Lerp(t.Y, MinPoint.Y, MaxPoint.Y));
    }

    public Vector2I Offset(Point2I p)
    {
      Point2I o = p - MinPoint;
      int ox = o.X;
      int oy = o.Y;
      if (MaxPoint.X > MinPoint.X)
      {
        ox /= (MaxPoint.X - MinPoint.X);
      }

      if (MaxPoint.Y > MinPoint.Y)
      {
        oy /= (MaxPoint.Y - MinPoint.Y);
      }

      return new Vector2I(ox, oy);
    }


    public void BoundingSphere(out Point2I c, out int radius)
    {
      c = (MinPoint + MaxPoint) / 2;
      radius = c.IsInside(this) ? c.Distance(MaxPoint) : 0;
    }

    /// <inheritdoc />
    public override string ToString()
    {
      return $"[ {MinPoint} - {MaxPoint} ]";
    }

    public Bounds2I Intersect(Bounds2I b2)
    {
        return new Bounds2I(Point2I.Max(MinPoint, b2.MinPoint), Point2I.Min(MaxPoint, b2.MaxPoint));
    }

    public Bounds2D ToBounds2D()
    {
      return new Bounds2D(MinPoint.ToPoint2D(), MaxPoint.ToPoint2D());
    }

    public IEnumerable<Point2I> GetPoints()
    {
      // handle case where min/max are degenerate
      if (MinPoint.X >= MaxPoint.X || MinPoint.Y >= MaxPoint.Y)
      {
        yield break;
      }

      for (int j = MinPoint.Y; j < MaxPoint.Y; j++)
      {
        for (int i = MinPoint.X; i < MaxPoint.X; i++)
        {
          yield return new Point2I(i, j);
        }
      }
    }
  }
}
