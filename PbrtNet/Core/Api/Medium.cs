using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Geometry;
using PbrtNet.Core.Interactions;

namespace PbrtNet.Core.Api
{
  public abstract class Medium
  {
    public abstract Spectrum Tr(Ray ray, Sampler sampler);
    public abstract Spectrum Sample(Ray ray, Sampler sampler, MediumInteraction mi);
  }
}
