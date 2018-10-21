using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Api;
using PbrtNet.Core.Geometry;
using PbrtNet.Core.Interactions;

namespace PbrtNet.Core.Integrators
{
  public abstract class SamplerIntegrator : Integrator
  {
    public SamplerIntegrator(Camera camera, Sampler sampler, Bounds2I pixelBounds)
    {
      Camera = camera;
      _sampler = sampler;
      _pixelBounds = pixelBounds;
    }

    protected Camera Camera { get; }
    private Sampler _sampler;
    private Bounds2I _pixelBounds;

    public virtual void PreProcess(Scene scene, Sampler sampler)
    {
    }

    public abstract Spectrum Li(RayDifferential ray, Scene scene, Sampler sampler, int depth = 0);

    public Spectrum SpecularReflect(
      RayDifferential ray,
      SurfaceInteraction isect,
      Scene scene,
      Sampler sampler,
      int depth)
    {
      // Compute specular reflection direction _wi_ and BSDF value
      Vector3D wo = isect.Wo;
      BxdfType type = BxdfType.Reflection| BxdfType.Specular;
      Spectrum f = isect.Bsdf.Sample_f(wo, out Vector3D wi, sampler.Get2D(), out double pdf, out BxdfType sampledType, type);

      // Return contribution of specular reflection
      Normal3D ns = isect.ShadingN;
      if (pdf > 0.0 && !f.IsBlack() && wi.AbsDot(ns) != 0.0)
      {
        // Compute ray differential _rd_ for specular reflection
        RayDifferential rd = new RayDifferential(isect.SpawnRay(wi));
        if (ray.HasDifferentials)
        {
          rd.HasDifferentials = true;
          rd.RxOrigin = isect.P + isect.Dpdx.ToPoint3D();
          rd.RyOrigin = isect.P + isect.Dpdy.ToPoint3D();
          // Compute differential reflected directions
          Normal3D dndx = isect.ShadingDndu * isect.Dudx +
                          isect.ShadingDndv * isect.Dvdx;
          Normal3D dndy = isect.ShadingDndu * isect.Dudy +
                          isect.ShadingDndv * isect.Dvdy;
          Vector3D dwodx = -ray.RxDirection - wo,
                   dwody = -ray.RyDirection - wo;
          double dDNdx =dwodx.Dot(ns) + wo.Dot(dndx);
          double dDNdy = dwody.Dot(ns) + wo.Dot(dndy);
          rd.RxDirection =
            wi - dwodx + 2.0 * (wo.Dot(ns) * dndx + dDNdx * ns).ToVector3D();
          rd.RyDirection =
            wi - dwody + 2.0 * (wo.Dot(ns) * dndy + dDNdy * ns).ToVector3D();
        }
        return f * Li(rd, scene, sampler, depth + 1) * wi.AbsDot(ns) /
               pdf;
      }
      else
      {
        return Spectrum.Create(0.0);
      }
    }

    public Spectrum SpecularTransmit(
      RayDifferential ray,
      SurfaceInteraction isect,
      Scene scene,
      Sampler sampler,
      int depth)
    {
      Vector3D wo = isect.Wo;
      double pdf;
      Point3D p = isect.P;
      Normal3D ns = isect.ShadingN;
      Bsdf bsdf = isect.Bsdf;
      Spectrum f = bsdf.Sample_f(wo, out Vector3D wi, sampler.Get2D(), out pdf,
                                 out BxdfType sampledType, BxdfType.Transmission | BxdfType.Specular);
      Spectrum L = Spectrum.Create(0.0);
      if (pdf > 0.0 && !f.IsBlack() && wi.AbsDot(ns) != 0.0)
      {
        // Compute ray differential _rd_ for specular transmission
        RayDifferential rd = new RayDifferential(isect.SpawnRay(wi));
        if (ray.HasDifferentials)
        {
          rd.HasDifferentials = true;
          rd.RxOrigin = p + isect.Dpdx.ToPoint3D();
          rd.RyOrigin = p + isect.Dpdy.ToPoint3D();

          double eta = bsdf.Eta;
          Vector3D w = -wo;
          if (wo.Dot(ns) < 0.0)
          {
            eta = 1.0 / eta;
          }

          Normal3D dndx = isect.ShadingDndu * isect.Dudx +
                          isect.ShadingDndv * isect.Dvdx;
          Normal3D dndy = isect.ShadingDndu * isect.Dudy +
                          isect.ShadingDndv * isect.Dvdy;

          Vector3D dwodx = -ray.RxDirection - wo,
                   dwody = -ray.RyDirection - wo;
          double dDNdx = dwodx.Dot(ns) + wo.Dot(dndx);
          double dDNdy = dwody.Dot(ns) + wo.Dot(dndy);

          double mu = eta * w.Dot(ns) - wi.Dot(ns);
          double dmudx =
            (eta - (eta * eta * w.Dot(ns)) / wi.Dot(ns)) * dDNdx;
          double dmudy =
            (eta - (eta * eta * w.Dot(ns)) / wi.Dot(ns)) * dDNdy;

          rd.RxDirection =
            wi + eta * dwodx - (mu * dndx + dmudx * ns).ToVector3D();
          rd.RyDirection =
            wi + eta * dwody - (mu * dndy + dmudy * ns).ToVector3D();
        }
        L = f * Li(rd, scene, sampler, depth + 1) * wi.AbsDot(ns) / pdf;
      }
      return L;

    }

    public override void Render(Scene scene)
    {
      throw new NotImplementedException();
    }
  }
}
