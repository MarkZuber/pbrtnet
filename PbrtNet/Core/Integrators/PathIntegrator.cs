using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Api;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Integrators
{
    public class PathIntegrator : SamplerIntegrator
    {
      private readonly int _maxDepth;
      private readonly double _rrThreshold;
      private readonly string _lightSampleStrategy;
      private readonly LightDistribution _lightDistribution;

      public static PathIntegrator Create(ParamSet paramSet, Sampler sampler, Camera camera)
      {
        int maxDepth = paramSet.FindOneInt("maxdepth", 5);
        Bounds2I pixelBounds = camera.Film.GetSampleBounds();
        int[] pb = paramSet.FindInt("pixelbounds");
        if (pb.Length > 0)
        {
          if (pb.Length != 4)
          {
            throw new InvalidOperationException();
          }

          pixelBounds = pixelBounds.Intersect(new Bounds2I(new Point2I(pb[0], pb[2]), new Point2I(pb[1], pb[3])));
          if (pixelBounds.Area == 0.0)
          {
            throw new InvalidOperationException("degenerate pixelbounds specified");
          }
        }

        double rrThreshold = paramSet.FindOneDouble("rrthreshold", 1.0);
        string lightStrategy = paramSet.FindOneString("lightsamplestrategy", "spatial");
        return new PathIntegrator(maxDepth, camera, sampler, pixelBounds, rrThreshold, lightStrategy);
      }

      /// <inheritdoc />
      public PathIntegrator(int maxDepth, Camera camera, Sampler sampler, Bounds2I pixelBounds, double rrThreshold = 1.0, string lightSampleStrategy = "spatial")
        : base(camera, sampler, pixelBounds)
      {
        _maxDepth = maxDepth;
      }

      /// <inheritdoc />
      public override Spectrum Li(RayDifferential ray, Scene scene, Sampler sampler, int depth = 0)
      {
        throw new NotImplementedException();
      }

      /// <inheritdoc />
      public override void PreProcess(Scene scene, Sampler sampler)
      {
        base.PreProcess(scene, sampler);
      }
    }
}
