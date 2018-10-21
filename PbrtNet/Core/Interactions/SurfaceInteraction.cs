using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Api;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Interactions
{
  public class SurfaceInteraction : Interaction
  {
    public SurfaceInteraction()
    {
    }

    public SurfaceInteraction(
      Point3D p,
      Vector3D pError,
      Point2D uv,
      Vector3D wo,
      Vector3D dpdu,
      Vector3D dpdv,
      Normal3D dndu,
      Normal3D dndv,
      double time,
      Shape shape,
      int faceIndex = 0) : base(p, (dpdu.Cross(dpdv).Normalize()).ToNormal3D(), pError, wo, time, null)
    {
      Uv = uv;
      Dpdu = dpdu;
      Dpdv = dpdv;
      Dndu = dndu;
      Dndv = dndv;
      Shape = shape;
      FaceIndex = faceIndex;

      ShadingN = N;
      ShadingDpdu = dpdu;
      ShadingDpdv = dpdv;
      ShadingDndu = dndu;
      ShadingDndv = dndv;

      // Adjust normal based on orientation and handedness
      if (shape != null &&
          (shape.ReverseOrientation ^ shape.TransformSwapsHandedness))
      {
        N *= -1;
        ShadingN *= -1;
      }
    }

    public void SetShadingGeometry(
      Vector3D dpdu,
      Vector3D dpdv,
      Normal3D dndu,
      Normal3D dndv,
      bool orientationIsAuthoritative)
    {
      // Compute _shading.n_ for _SurfaceInteraction_
      ShadingN = dpdu.Cross(dpdv).Normalize().ToNormal3D();
      if (Shape != null && (Shape.ReverseOrientation ^ Shape.TransformSwapsHandedness))
      {
        ShadingN = -ShadingN;
      }

      if (orientationIsAuthoritative)
      {
        N = N.FaceForward(ShadingN);
      }
      else
      {
        ShadingN = ShadingN.FaceForward(N);
      }

      // Initialize _shading_ partial derivative values
      ShadingDpdu = dpdu;
      ShadingDpdv = dpdv;
      ShadingDndu = dndu;
      ShadingDndv = dndv;
    }

    public void ComputeScatteringFunctions(
      RayDifferential ray,
      bool allowMultipleLobes = false,
      TransportMode mode = TransportMode.Radiance)
    {
      ComputeDifferentials(ray);
      Primitive.ComputeScatteringFunctions(this, mode, allowMultipleLobes);
    }

    public void ComputeDifferentials(RayDifferential ray)
    {
      if (ray.HasDifferentials)
      {
        try
        {
          // Estimate screen space change in $\pt{}$ and $(u,v)$

          // Compute auxiliary intersection points with plane
          double d = N.Dot(P.ToVector3D());
          double tx = -(N.Dot(ray.RxOrigin.ToVector3D()) - d) / N.Dot(ray.RxDirection);
          if (double.IsInfinity(tx) || double.IsNaN(tx))
          {
            throw new InvalidOperationException();
          }

          Point3D px = (ray.RxOrigin.ToVector3D() + tx * ray.RxDirection).ToPoint3D();
          double ty = -(N.Dot(ray.RyOrigin.ToVector3D()) - d) / N.Dot(ray.RyDirection);
          if (double.IsInfinity(ty) || double.IsNaN(ty))
          {
            throw new InvalidOperationException();
          }

          Point3D py = (ray.RyOrigin.ToVector3D() + ty * ray.RyDirection).ToPoint3D();
          Dpdx = (px - P).ToVector3D();
          Dpdy = (py - P).ToVector3D();

          // Compute $(u,v)$ offsets at auxiliary points

          // Choose two dimensions to use for ray offset computation
          int[] dim = new int[2];
          if (Math.Abs(N.X) > Math.Abs(N.Y) && Math.Abs(N.X) > Math.Abs(N.Z))
          {
            dim[0] = 1;
            dim[1] = 2;
          }
          else if (Math.Abs(N.Y) > Math.Abs(N.Z))
          {
            dim[0] = 0;
            dim[1] = 2;
          }
          else
          {
            dim[0] = 0;
            dim[1] = 1;
          }

          // Initialize _A_, _Bx_, and _By_ matrices for offset computation
          double[,] a = new double[2, 2];
          a[0, 0] = Dpdu[dim[0]];
          a[0, 1] = Dpdv[dim[0]];
          a[1, 0] = Dpdu[dim[1]];
          a[1, 1] = Dpdv[dim[1]];

          //Float A[2][2] = {{dpdu[dim[0]], dpdv[dim[0]]
          //  },
          //  {dpdu[dim[1]], dpdv[dim[1]]
          //  }};
          double[] bx = new double[2];
          bx[0] = px[dim[0]] - P[dim[0]];
          bx[1] = px[dim[1]] - P[dim[1]];

          double[] by = new double[2];
          by[0] = py[dim[0]] - P[dim[0]];
          by[1] = py[dim[1]] - P[dim[1]];

          if (PbrtMath.SolveLinearSystem2x2(a, bx, out double du1, out double dv1))
          {
            Dudx = du1;
            Dvdx = dv1;
          }
          else
          {
            Dudx = 0.0;
            Dvdx = 0.0;
          }

          if (PbrtMath.SolveLinearSystem2x2(a, by, out double du2, out double dv2))
          {
            Dudx = du2;
            Dvdx = dv2;
          }
          else
          {
            Dudy = 0.0;
            Dvdy = 0.0;
          }
        }
        catch (InvalidOperationException)
        {
          Dudx = 0.0;
          Dvdx = 0.0;
          Dudy = 0.0;
          Dvdy = 0.0;
          Dpdx = new Vector3D();
          Dpdy = new Vector3D();
        }
      } else {
        Dudx = 0.0;
        Dvdx = 0.0;
        Dudy = 0.0;
        Dvdy = 0.0;
        Dpdx = new Vector3D();
        Dpdy = new Vector3D();
      }
    }

    public Spectrum Le(Vector3D w)
    {
      AreaLight area = Primitive.GetAreaLight();
      return area != null ? area.L(this, w) : Spectrum.Create(0.0);
    }

    public Point2D Uv { get; }
    public Vector3D Dpdu { get; }
    public Vector3D Dpdv { get; }
    public Normal3D Dndu { get; }
    public Normal3D Dndv { get; }
    public Shape Shape { get; } = null;
    public Primitive Primitive { get; } = null;
    public Bsdf Bsdf { get; } = null;
    public Bssrdf Bssrdf { get; } = null;
    public Vector3D Dpdx { get; set; }
    public Vector3D Dpdy { get; set; }
    public double Dudx { get; set; }
    public double Dvdx { get; set; }
    public double Dudy { get; set; }
    public double Dvdy { get; set; }

    public Normal3D ShadingN { get; private set; }
    public Vector3D ShadingDpdu { get; private set; }
    public Vector3D ShadingDpdv { get; private set; }
    public Normal3D ShadingDndu { get; private set; }
    public Normal3D ShadingDndv { get; private set; }

    public int FaceIndex { get; } = 0;
  }
}
