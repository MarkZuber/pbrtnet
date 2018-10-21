// -----------------------------------------------------------------------
// <copyright file="Primitive.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using PbrtNet.Core.Geometry;
using PbrtNet.Core.Interactions;

namespace PbrtNet.Core.Api
{
  public abstract class Primitive
  {
    public abstract Bounds3D WorldBound();
    public abstract bool Intersect(Ray r, out SurfaceInteraction isect);
    public abstract bool IntersectP(Ray r);
    public abstract AreaLight GetAreaLight();
    public abstract Material GetMaterial();

    public abstract void ComputeScatteringFunctions(
      SurfaceInteraction isect,
      TransportMode mode,
      bool allowMultipleLobes);
  }
}