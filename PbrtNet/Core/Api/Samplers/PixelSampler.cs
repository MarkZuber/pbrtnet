using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Api.Samplers
{
  public class PixelSampler : Sampler 
  {
    /// <inheritdoc />
    public PixelSampler(int samplesPerPixel)
      : base(samplesPerPixel)
    {
    }

    /// <inheritdoc />
    public override double Get1D()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Point2D Get2D()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public override Sampler Clone(int seed)
    {
      throw new NotImplementedException();
    }
  }
}
