using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Api;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Samplers
{
  public class RandomSampler : Sampler
  {
    private readonly Random _rng;

    /// <inheritdoc />
    public RandomSampler(int samplesPerPixel, int seed = 0)
      : base(samplesPerPixel)
    {
      _rng = seed == 0 ? new Random() : new Random(seed);
    }

    /// <inheritdoc />
    public override double Get1D()
    {
      //ProfilePhase _(Prof::GetSample);
      //CHECK_LT(currentPixelSampleIndex, samplesPerPixel);
      return _rng.NextDouble();
    }

    /// <inheritdoc />
    public override Point2D Get2D()
    {
      //ProfilePhase _(Prof::GetSample);
      //CHECK_LT(currentPixelSampleIndex, samplesPerPixel);
      return new Point2D(_rng.NextDouble(), _rng.NextDouble());
    }

    /// <inheritdoc />
    public override Sampler Clone(int seed)
    {
      RandomSampler rs = new RandomSampler(_samplesPerPixel, seed);
      return rs;
    }

    /// <inheritdoc />
    public override void StartPixel(Point2I p)
    {
      //ProfilePhase _(Prof::StartPixel);
      for (int i = 0; i < _sampleArray1D.Count; ++i)
      {
        for (int j = 0; j < _sampleArray1D[i].Count; ++j)
        {
          _sampleArray1D[i][j] = _rng.NextDouble();
        }
      }

      for (int i = 0; i < _sampleArray2D.Count; ++i)
      {
        for (int j = 0; j < _sampleArray2D[i].Count; ++j)
        {
          _sampleArray2D[i][j] = new Point2D(_rng.NextDouble(), _rng.NextDouble());

        }
      }

      base.StartPixel(p);
    }

    public static RandomSampler Create(ParamSet paramSet)
    {
      int ns = paramSet.FindOneInt("pixelsamples", 4);
      return new RandomSampler(ns);
    }
  }
}
