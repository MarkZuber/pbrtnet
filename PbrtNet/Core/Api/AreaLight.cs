using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Geometry;
using PbrtNet.Core.Interactions;

namespace PbrtNet.Core.Api
{
    public abstract class AreaLight : Light
    {
      protected AreaLight(Transform lightToWorld, MediumInterface medium, int nSamples) : base(LightFlags.Area, lightToWorld, medium, nSamples)
      {
      }

      public abstract Spectrum L(Interaction intr, Vector3D w);
  }
}
