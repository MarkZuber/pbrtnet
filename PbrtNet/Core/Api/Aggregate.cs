using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Interactions;

namespace PbrtNet.Core.Api
{
  public abstract class Aggregate : Primitive
  {
    /// <inheritdoc />
    public override void ComputeScatteringFunctions(SurfaceInteraction isect, TransportMode mode, bool allowMultipleLobes)
    {
      throw new InvalidOperationException("Aggregate.ComputeScatteringFunctions() method called; should have gone to GeometricPrimitive");
    }

    /// <inheritdoc />
    public override AreaLight GetAreaLight()
    {
      throw new InvalidOperationException("Aggregate.GetAreaLight() method called; should have gone to GeometricPrimitive");
    }

    /// <inheritdoc />
    public override Material GetMaterial()
    {
      throw new InvalidOperationException("Aggregate.GetMaterial() method called; should have gone to GeometricPrimitive");
    }
  }
}
