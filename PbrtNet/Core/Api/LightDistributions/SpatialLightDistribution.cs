using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Api.LightDistributions
{
    public class SpatialLightDistribution : LightDistribution
    {
      private readonly Scene _scene;

      public SpatialLightDistribution(Scene scene)
      {
        _scene = scene;
      }

      /// <inheritdoc />
    public override Distribution1D Lookup(Point3D p)
      {
        throw new NotImplementedException();
      }
    }
}
