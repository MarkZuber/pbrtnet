// -----------------------------------------------------------------------
// <copyright file="Scene.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using PbrtNet.Core.Geometry;
using PbrtNet.Core.Interactions;

namespace PbrtNet.Core.Api
{
  public class Scene
  {
    private readonly Primitive _aggregate;
    private readonly List<Light> _infiniteLights = new List<Light>();
    private readonly List<Light> _lights;

    public Scene(Primitive aggregate, IEnumerable<Light> lights)
    {
      _aggregate = aggregate;
      _lights = lights.ToList();

      // Scene Constructor Implementation
      WorldBound = _aggregate.WorldBound();
      foreach (var light in _lights)
      {
        light.Preprocess(this);
        if ((light.Flags & LightFlags.Infinite) == LightFlags.Infinite)
        {
          _infiniteLights.Add(light);
        }
      }
    }

    public int NumLights => _lights.Count;
    public IEnumerable<Light> Lights => _lights;

    public Bounds3D WorldBound { get; }

    public bool Intersect(Ray ray, out SurfaceInteraction isect)
    {
      // ++nIntersectionTests;
      // DCHECK_NE(ray.d, Vector3f(0, 0, 0));
      return _aggregate.Intersect(ray, out isect);
    }

    public bool IntersectP(Ray ray)
    {
      //++nShadowTests;
      //DCHECK_NE(ray.d, Vector3f(0, 0, 0));
      return _aggregate.IntersectP(ray);
    }

    public bool IntersectTr(Ray ray, Sampler sampler, out SurfaceInteraction isect, out Spectrum transmittance)
    {
      transmittance = Spectrum.Create(1.0);
      while (true)
      {
        bool hitSurface = Intersect(ray, out isect);
        // Accumulate beam transmittance for ray segment
        if (ray.Medium != null)
        {
          transmittance *= ray.Medium.Tr(ray, sampler);
        }

        // Initialize next ray segment or terminate transmittance computation
        if (!hitSurface)
        {
          return false;
        }

        if (isect.Primitive.GetMaterial() != null)
        {
          return true;
        }

        ray = isect.SpawnRay(ray.Direction);
      }
    }
  }
}