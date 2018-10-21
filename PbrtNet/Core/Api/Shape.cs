using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Geometry;
using PbrtNet.Core.Interactions;

namespace PbrtNet.Core.Api
{
    public abstract class Shape
    {
      protected Shape(Transform objectToWorld, Transform worldToObject, bool reverseOrientation)
      {
        ObjectToWorld = objectToWorld;
        WorldToObject = worldToObject;
        ReverseOrientation = reverseOrientation;
        TransformSwapsHandedness = ObjectToWorld.SwapsHandedness;
      }

      public abstract Bounds3D ObjectBound();

      public virtual Bounds3D WorldBound()
      {
        return ObjectToWorld.AtBounds(ObjectBound());
      }

      public abstract bool Intersect(
        Ray ray,
        out double tHit,
        out SurfaceInteraction isect,
        bool testAlphaTexture = true);

      public virtual bool IntersectP(Ray ray, bool testAlphaTexture = true)
      {
        return Intersect(ray, out double thit, out SurfaceInteraction isect, testAlphaTexture);
      }

      public abstract double Area();

      // Sample a point on the surface of the shape and return the PDF with
      // respect to area on the surface.
      public abstract Interaction Sample(Point2D u, out double pdf);

      public virtual double Pdf(Interaction it) {
        return 1.0 / Area();
      }

      // Sample a point on the shape given a reference point |ref| and
      // return the PDF with respect to solid angle from |ref|.
      public virtual Interaction Sample(Interaction it, Point2D u, out double pdf)
      {
        Interaction intr = Sample(u, out pdf);
        Vector3D wi = (intr.P - it.P).ToVector3D();
        if (wi.LengthSquared() == 0.0)
        {
          pdf = 0.0;
        }
        else
        {
          wi = wi.Normalize();
          // Convert from area measure, as returned by the Sample() call
          // above, to solid angle measure.
          pdf *= it.P.DistanceSquared(intr.P) / intr.N.AbsDot(-wi);
          if (double.IsInfinity(pdf))
          {
            pdf = 0.0;
          }
        }
        return intr;
    }

    public virtual double Pdf(Interaction it, Vector3D wi)
      {
        // Intersect sample ray with area light geometry
        Ray ray = it.SpawnRay(wi);

        // Ignore any alpha textures used for trimming the shape when performing
        // this intersection. Hack for the "San Miguel" scene, where this is used
        // to make an invisible area light.
        if (!Intersect(ray, out double tHit, out SurfaceInteraction isectLight, false))
        {
          return 0.0;
        }

        // Convert light sample weight to solid angle measure
        double pdf = it.P.DistanceSquared(isectLight.P) /
        (isectLight.N.AbsDot(-wi) * Area());
        if (double.IsInfinity(pdf))
        {
          pdf = 0.0;
        }
        return pdf;
    }

    // Returns the solid angle subtended by the shape w.r.t. the reference
    // point p, given in world space. Some shapes compute this value in
    // closed-form, while the default implementation uses Monte Carlo
    // integration; the nSamples parameter determines how many samples are
    // used in this case.
    public virtual double SolidAngle(Point3D p, int nSamples = 512)
      {
        Interaction it = new Interaction(p, new Normal3D(), new Vector3D(), new Vector3D(0.0, 0.0, 1.0), 0.0,
                          new MediumInterface());

        double solidAngle = 0.0;
        for (int i = 0; i < nSamples; ++i)
        {
          Point2D u = new Point2D(PbrtMath.RadicalInverse(0, i), PbrtMath.RadicalInverse(1, i));
          double pdf;
          Interaction pShape = Sample(it, u, out pdf);
          if (pdf > 0.0 && !IntersectP(new Ray(p, (pShape.P - p).ToVector3D(), 0.999)))
          {
            solidAngle += 1.0 / pdf;
          }
        }
        return solidAngle / nSamples;

    }

    // Shape Public Data
    public Transform ObjectToWorld { get; }
      public Transform WorldToObject { get; }
      public bool ReverseOrientation { get; }
      public bool TransformSwapsHandedness { get; }
    }
}
