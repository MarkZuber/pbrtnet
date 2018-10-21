using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Geometry;
using PbrtNet.Core.Interactions;

namespace PbrtNet.Core.Api
{
  public class VisibilityTester
  {
    public VisibilityTester() { }
    // VisibilityTester Public Methods
    public VisibilityTester(Interaction p0, Interaction p1)
    {
      P0 = p0;
      P1 = p1;
    }
    public Interaction P0 { get; }
    public Interaction P1 { get;}

    public bool Unoccluded(Scene scene)
    {
      return !scene.IntersectP(P0.SpawnRayTo(P1));
    }

    public Spectrum Tr(Scene scene, Sampler sampler)
    {
      Ray ray = new Ray(P0.SpawnRayTo(P1));
      Spectrum Tr = Spectrum.Create(1.0);
      while (true)
      {
        SurfaceInteraction isect;
        bool hitSurface = scene.Intersect(ray, out isect);
        // Handle opaque surface along ray's path
        if (hitSurface && isect.Primitive.GetMaterial() != null)
        {
          return Spectrum.Create(0.0);
        }

        // Update transmittance for current ray segment
        if (ray.Medium != null)
        {
          Tr *= ray.Medium.Tr(ray, sampler);
        }

        // Generate next ray segment or return final transmittance
        if (!hitSurface)
        {
          break;
        }

        ray = isect.SpawnRayTo(P1);
      }
      return Tr;

    }
  }
}
