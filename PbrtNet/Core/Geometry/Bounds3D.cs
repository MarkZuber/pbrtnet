using System;
using System.Collections.Generic;
using System.Text;

namespace PbrtNet.Core.Geometry
{
    public class Bounds3D
    {
      public Bounds3D()
      {
        MinPoint = new Point3D(-double.MaxValue, -double.MaxValue, -double.MaxValue);
        MaxPoint = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
      }

      public Bounds3D(Point3D p1, Point3D p2)
      {
        MinPoint = new Point3D(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y), Math.Min(p1.Z, p2.Z));
        MaxPoint = new Point3D(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y), Math.Max(p1.Z, p2.Z));
      }

      public Bounds3D(Point3D p1) : this(p1, p1)
      {
      }

      public Point3D MinPoint { get; }
      public Point3D MaxPoint { get; }

      /// <inheritdoc />
      public override string ToString()
      {
        return $"[ {MinPoint} - {MaxPoint} ]";
      }

      public Point3D this[int i]
      {
        get
        {
          switch (i)
          {
          case 0:
            return MinPoint;
          case 1: return MaxPoint;
          default:
            throw new IndexOutOfRangeException();
          }
        }
      }

      public Point3D Corner(int corner)
      {
        return new Point3D(this[corner & 1].X, this[(corner & 2) > 0 ? 1 : 0].Y, this[(corner & 4) > 0 ? 1 : 0].Z);
      }

      public Vector3D Diagonal => (MaxPoint - MinPoint).ToVector3D();

      public double SurfaceArea
      {
        get
        {
          Vector3D d = Diagonal;
          return 2.0 * (d.X * d.Y + d.X * d.Z + d.Y * d.Z);
        }
      }

      public double Volume
      {
        get
        {
          Vector3D d = Diagonal;
          return d.X * d.Y * d.Z;
        }
      }

      public int MaximumExtent
      {
        get
        {
          Vector3D d = Diagonal;
          if (d.X > d.Y && d.X > d.Z)
          {
            return 0;
          }
          else if (d.Y > d.Z)
          {
            return 1;
          }
          else
          {
            return 2;
          }
        }
      }

      public Point3D Lerp(Point3D t)
      {
        return new Point3D(PbrtMath.Lerp(t.X, MinPoint.X, MaxPoint.X),
          PbrtMath.Lerp(t.Y, MinPoint.Y, MaxPoint.Y),
          PbrtMath.Lerp(t.Z, MinPoint.Z, MaxPoint.Z));
      }

      public Vector3D Offset(Point3D p)
      {
        Vector3D o = (p - MinPoint).ToVector3D();
        double ox = o.X;
        double oy = o.Y;
        double oz = o.Z;

        if (MaxPoint.X > MinPoint.X)
        {
          ox /= MaxPoint.X - MinPoint.X;
        }

        if (MaxPoint.Y > MinPoint.Y)
        {
          oy /= MaxPoint.Y - MinPoint.Y;
        }

        if (MaxPoint.Z > MinPoint.Z)
        {
          oz /= MaxPoint.Z - MinPoint.Z;
        }

        return new Vector3D(ox, oy, oz);
    }

      public void BoundingSphere(out Point3D center, out double radius)
      {
        center = (MinPoint + MaxPoint) / 2.0;
        radius = center.IsInside(this) ? center.Distance(MaxPoint) : 0;
    }

      public Bounds3D Union(Point3D p)
      {
        return new Bounds3D(Point3D.Min(MinPoint, p), Point3D.Max(MaxPoint, p));
      }

      public Bounds3D Union(Bounds3D b2)
      {
        return new Bounds3D(Point3D.Min(MinPoint, b2.MinPoint), Point3D.Max(MaxPoint, b2.MaxPoint));
      }

      public Bounds3D Intersect(Bounds3D b2)
      {
        return new Bounds3D(Point3D.Max(MinPoint, b2.MinPoint), Point3D.Min(MaxPoint, b2.MaxPoint));
      }

      public bool Overlaps(Bounds3D b2)
      {
        bool x = (MaxPoint.X >= b2.MinPoint.X) && (MinPoint.X <= b2.MaxPoint.X);
        bool y = (MaxPoint.Y >= b2.MinPoint.Y) && (MinPoint.Y <= b2.MaxPoint.Y);
        bool z = (MaxPoint.Z >= b2.MinPoint.Z) && (MinPoint.Z <= b2.MaxPoint.Z);
        return (x && y && z);

    }

      public Bounds3D Expand(double delta)
      {
        return new Bounds3D(MinPoint - new Point3D(delta, delta, delta),
          MaxPoint + new Point3D(delta, delta, delta));
      }


    public bool IntersectP(Ray ray, out double hitt0, out double hitt1)
      {
        double t0 = 0, t1 = ray.TMax;
        for (int i = 0; i < 3; ++i)
        {
        // Update interval for _i_th bounding box slab
          double invRayDir = 1 / ray.Direction[i];
          double tNear = (MinPoint[i] - ray.Origin[i]) * invRayDir;
          double tFar = (MaxPoint[i] - ray.Origin[i]) * invRayDir;

          // Update parametric interval from slab intersection $t$ values
          if (tNear > tFar)
          {
            double temp = tNear;
            tNear = tFar;
            tFar = temp;
          }

          // Update _tFar_ to ensure robust ray--bounds intersection
          tFar *= 1 + 2 * PbrtMath.Gamma(3);
          t0 = tNear > t0 ? tNear : t0;
          t1 = tFar < t1 ? tFar : t1;
          if (t0 > t1)
          {
            hitt0 = 0.0;
            hitt1 = 0.0;
            return false;
          }
        }
        hitt0 = t0;
        hitt1 = t1;
        return true;

    }

    public bool IntersectP(Ray ray, Vector3D invDir, bool[] dirIsNeg)
      {
        // Check for ray intersection against $x$ and $y$ slabs
        double tMin = (this[dirIsNeg[0] ? 1 : 0].X - ray.Origin.X) * invDir.X;
        double tMax = (this[dirIsNeg[0] ? 0 : 1].X - ray.Origin.X) * invDir.X;
        double tyMin = (this[dirIsNeg[1] ? 1 : 0].Y - ray.Origin.Y) * invDir.Y;
        double tyMax = (this[dirIsNeg[1] ? 0 : 1].Y - ray.Origin.Y) * invDir.Y;

        // Update _tMax_ and _tyMax_ to ensure robust bounds intersection
        tMax *= 1.0 + 2.0 * PbrtMath.Gamma(3);
        tyMax *= 1.0 + 2.0 * PbrtMath.Gamma(3);
        if (tMin > tyMax || tyMin > tMax)
        {
          return false;
        }

        if (tyMin > tMin)
        {
          tMin = tyMin;
        }

        if (tyMax < tMax)
        {
          tMax = tyMax;
        }

        // Check for ray intersection against $z$ slab
        double tzMin = (this[dirIsNeg[2] ? 1 : 0].Z - ray.Origin.Z) * invDir.Z;
        double tzMax = (this[dirIsNeg[2] ? 0 : 1].Z - ray.Origin.Z) * invDir.Z;

        // Update _tzMax_ to ensure robust bounds intersection
        tzMax *= 1.0 + 2.0 * PbrtMath.Gamma(3);
        if (tMin > tzMax || tzMin > tMax)
        {
          return false;
        }

        if (tzMin > tMin)
        {
          tMin = tzMin;
        }

        if (tzMax < tMax)
        {
          tMax = tzMax;
        }

        return (tMin < ray.TMax) && (tMax > 0.0);
    }
  }
}
