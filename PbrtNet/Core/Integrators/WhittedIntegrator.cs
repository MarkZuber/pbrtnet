using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Api;
using PbrtNet.Core.Geometry;
using PbrtNet.Core.Interactions;

namespace PbrtNet.Core.Integrators
{
  public class WhittedIntegrator : SamplerIntegrator
  {
    private readonly int _maxDepth;

    public static WhittedIntegrator Create(ParamSet paramSet, Sampler sampler, Camera camera)
    {
      int maxDepth = paramSet.FindOneInt("maxdepth", 5);
      int[] pb = paramSet.FindInt("pixelbounds");
      Bounds2I pixelBounds = camera.Film.GetSampleBounds();
      if (pb != null)
      {
        if (pb.Length != 4)
        {
          //Error("Expected four values for \"pixelbounds\" parameter. Got %d.",
          //      np);
        }
        else
        {
          pixelBounds = pixelBounds.Intersect(new Bounds2I(new Point2I(pb[0], pb[2]), new Point2I(pb[1], pb[3])));
          if (pixelBounds.Area == 0)
          {
            //Error("Degenerate \"pixelbounds\" specified.");
          }
        }
      }
      return new WhittedIntegrator(maxDepth, camera, sampler, pixelBounds);

    }

    /// <inheritdoc />
    public WhittedIntegrator(int maxDepth, Camera camera, Sampler sampler, Bounds2I pixelBounds)
      : base(camera, sampler, pixelBounds)
    {
      _maxDepth = maxDepth;
    }

    /// <inheritdoc />
    public override Spectrum Li(RayDifferential ray, Scene scene, Sampler sampler, int depth = 0)
    {
      Spectrum L = Spectrum.Create(0.0);
      // Find closest ray intersection or return background radiance
      if (!scene.Intersect(ray, out SurfaceInteraction isect))
      {
        foreach (var light in scene.Lights)
        {
          L += light.Le(ray);
        }
        return L;
      }

      // Compute emitted and reflected light at ray intersection point

      // Initialize common variables for Whitted integrator
      Normal3D n = isect.ShadingN;
      Vector3D wo = isect.Wo;

      // Compute scattering functions for surface interaction
      isect.ComputeScatteringFunctions(ray);
      if (isect.Bsdf == null)
      {
        return Li(new RayDifferential(isect.SpawnRay(ray.Direction)), scene, sampler, depth);
      }

      // Compute emitted light if ray hit an area light source
      L += isect.Le(wo);

      // Add contribution of each light source
      foreach (var light in scene.Lights) {
        Spectrum Li =
          light.Sample_Li(isect, sampler.Get2D(), out Vector3D wi, out double pdf, out VisibilityTester visibility);
        if (Li.IsBlack() || pdf == 0.0)
        {
          continue;
        }

        Spectrum f = isect.Bsdf.f(wo, wi);
        if (!f.IsBlack() && visibility.Unoccluded(scene))
        {
          L += f * Li * wi.AbsDot(n) / pdf;
        }
      }
      if (depth + 1 < _maxDepth)
      {
        // Trace rays for specular reflection and refraction
        L += SpecularReflect(ray, isect, scene, sampler, depth);
        L += SpecularTransmit(ray, isect, scene, sampler, depth);
      }
      return L;
    }
  }
}
