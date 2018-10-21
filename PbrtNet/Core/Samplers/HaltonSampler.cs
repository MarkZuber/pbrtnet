//using System;
//using System.Collections.Generic;
//using System.Text;
//using PbrtNet.Core.Api;
//using PbrtNet.Core.Api.Samplers;
//using PbrtNet.Core.Geometry;

//namespace PbrtNet.Core.Samplers
//{
//  public class HaltonSampler : GlobalSampler
//  {
//    private const int KMaxResolution = 128;

//    /// <inheritdoc />
//    public HaltonSampler(int samplesPerPixel, Bounds2I sampleBounds,
//    bool sampleAtPixelCenter = false)
//      : base(samplesPerPixel)
//    {
//      _sampleAtPixelCenter = sampleAtPixelCenter;

//      // Generate random digit permutations for Halton sampler
//      if (_radicalInversePermutations.Count == 0)
//      {
//        _radicalInversePermutations = PbrtMath.ComputeRadicalInversePermutations(new Random());
//      }

//      // Find radical inverse base scales and exponents that cover sampling area
//      Vector2I res = (sampleBounds.MaxPoint- sampleBounds.MinPoint).ToVector2I();
//      for (int i = 0; i < 2; ++i)
//      {
//        int thebase = (i == 0) ? 2 : 3;
//        int scale = 1, exp = 0;
//        while (scale < Math.Min(res[i], KMaxResolution))
//        {
//          scale *= thebase;
//          ++exp;
//        }
//        _baseScales[i] = scale;
//        _baseExponents[i] = exp;
//      }

//      // Compute stride in samples for visiting each pixel area
//      _sampleStride = _baseScales[0] * _baseScales[1];

//      // Compute multiplicative inverses for _baseScales_
//      _multInverse[0] = MultiplicativeInverse(_baseScales[1], _baseScales[0]);
//      _multInverse[1] = MultiplicativeInverse(_baseScales[0], _baseScales[1]);

//    }

//    private static int Mod(int a, int b)
//    {
//      int result = a - (a / b) * b;
//      return result < 0 ? result + b : result;
//    }

//    private static int MultiplicativeInverse(int a, int n)
//    {
//      extendedGCD(a, n, out int x, out int y);
//      return Mod(x, n);
//    }

//    static void extendedGCD(int a, int b, out int x, out int y)
//    {
//      if (b == 0)
//      {
//        x = 1;
//        y = 0;
//        return;
//      }
//      int d = a / b;
//      extendedGCD(b, a % b, out int xp, out int yp);
//      x = yp;
//      y = xp - (d * yp);
//    }


//    public long GetIndexForSample(long sampleNum)
//    {
//      if (currentPixel != _pixelForOffset)
//      {
//        // Compute Halton sample offset for _currentPixel_
//        _offsetForCurrentPixel = 0;
//        if (_sampleStride > 1)
//        {
//          Point2I pm = new Point2I(Mod(currentPixel[0], KMaxResolution),
//                                   Mod(currentPixel[1], KMaxResolution));
//          for (int i = 0; i < 2; ++i)
//          {
//            long dimOffset =
//              (i == 0)
//                ? InverseRadicalInverse < 2 > (pm[i], baseExponents[i])
//                : InverseRadicalInverse < 3 > (pm[i], baseExponents[i]);
//            _offsetForCurrentPixel +=
//              dimOffset * (_sampleStride / _baseScales[i]) * _multInverse[i];
//          }
//          _offsetForCurrentPixel %= _sampleStride;
//        }
//        _pixelForOffset = currentPixel;
//      }
//      return _offsetForCurrentPixel + sampleNum * _sampleStride;

//    }

//    public double SampleDimension(long index, int dimension)
//    {
//      if (_sampleAtPixelCenter && (dimension == 0 || dimension == 1))
//      {
//        return 0.5f;
//      }

//      if (dimension == 0)
//      {
//        return RadicalInverse(dimension, index >> _baseExponents[0]);
//      }
//      else if (dimension == 1)
//      {
//        return RadicalInverse(dimension, index / _baseScales[1]);
//      }
//      else
//      {
//        return ScrambledRadicalInverse(dimension, index,
//                                       PermutationForDimension(dimension));
//      }
//    }

//    public static HaltonSampler Create(PbrtOptions options, ParamSet paramSet,
//    Bounds2I sampleBounds) {
//      int nsamp = paramSet.FindOneInt("pixelsamples", 16);
//      if (options.QuickRender)
//      {
//        nsamp = 1;
//      }

//      bool sampleAtCenter = paramSet.FindOneBool("samplepixelcenter", false);
//      return new HaltonSampler(nsamp, sampleBounds, sampleAtCenter);
//    }

//    public override Sampler Clone(int seed)
//    {
//      return new HaltonSampler(this);
//    }

//    // HaltonSampler Private Data
//    private static List<UInt16> _radicalInversePermutations;
//    private Point2I _baseScales, _baseExponents;
//    private int _sampleStride;
//    private int[] _multInverse = new int[2];
//    private Point2I _pixelForOffset = new Point2I(int.MaxValue, int.MaxValue);
//    private long _offsetForCurrentPixel;
//    // Added after book publication: force all image samples to be at the
//    // center of the pixel area.
//    private bool _sampleAtPixelCenter;

//    // HaltonSampler Private Methods
//    private UInt16 PermutationForDimension(int dim) {
//      if (dim >= PbrtMath.PrimeTableSize)
//      {
//        //LOG(FATAL) << StringPrintf("HaltonSampler can only sample %d dimensions.", PrimeTableSize);
//      }

//      return _radicalInversePermutations[PbrtMath.PrimeSums[dim]];
//    }

//}
//}
