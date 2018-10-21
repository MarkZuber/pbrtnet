// -----------------------------------------------------------------------
// <copyright file="LightDistribution.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System;
using PbrtNet.Core.Api.LightDistributions;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Api
{
  public abstract class LightDistribution
  {
    // Given a point |p| in space, this method returns a (hopefully
    // effective) sampling distribution for light sources at that point.
    public abstract Distribution1D Lookup(Point3D p);

    public static LightDistribution Create(string name, Scene scene)
    {
      if (string.Compare(name, "uniform", StringComparison.OrdinalIgnoreCase) == 0 || scene.NumLights == 1)
      {
        return new UniformLightDistribution(scene);
      }
      if (string.Compare(name, "power", StringComparison.OrdinalIgnoreCase) == 0)
      {
        return new PowerLightDistribution(scene);
      }
      if (string.Compare(name, "spatial", StringComparison.OrdinalIgnoreCase) == 0)
      {
        return new SpatialLightDistribution(scene);
      }
    
      // TODO: ERROR
      return new SpatialLightDistribution(scene);
    }
  }
}