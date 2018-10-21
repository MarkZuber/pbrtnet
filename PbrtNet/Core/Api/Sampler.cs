using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Api
{
  public abstract class Sampler
  {
    public Sampler(int samplesPerPixel)
    {
      _samplesPerPixel = samplesPerPixel;
    }

    public virtual void StartPixel(Point2I p)
    {
      currentPixel = p;
      CurrentPixelSampleIndex = 0;
      // Reset array offsets for next pixel sample
      _array1DOffset = _array2DOffset = 0;
    }

    public abstract double Get1D();
    public abstract Point2D Get2D();

    public CameraSample GetCameraSample(Point2I pRaster)
    {
      CameraSample cs = new CameraSample(pRaster.ToPoint2D() + Get2D(), Get2D(), Get1D());
      return cs;
    }

    public void Request1DArray(int n)
    {
      //CHECK_EQ(RoundCount(n), n);
      _samples1DArraySizes.Add(n);
      _sampleArray1D.Add(new List<double>(Convert.ToInt32(n * _samplesPerPixel)));
    }

    public void Request2DArray(int n)
    {
      //CHECK_EQ(RoundCount(n), n);
      _samples2DArraySizes.Add(n);
      _sampleArray2D.Add(new List<Point2D>(Convert.ToInt32(n * _samplesPerPixel)));
    }

    public virtual int RoundCount(int n) { return n; }

    //public double Get1DArray(int n)
    //{
    //  if (_array1DOffset == _sampleArray1D.Count)
    //  {
    //    return null;
    //  }

    //  //CHECK_EQ(samples1DArraySizes[array1DOffset], n);
    //  //CHECK_LT(currentPixelSampleIndex, samplesPerPixel);
    //  return _sampleArray1D[_array1DOffset++][CurrentPixelSampleIndex * n];
    //}

    public Point2D Get2DArray(int n)
    {
      if (_array2DOffset == _sampleArray2D.Count)
      {
        return null;
      }

      //CHECK_EQ(samples2DArraySizes[array2DOffset], n);
      //CHECK_LT(currentPixelSampleIndex, _samplesPerPixel);
      return _sampleArray2D[_array2DOffset++][CurrentPixelSampleIndex * n];
    }

    public virtual bool StartNextSample()
    {
      // Reset array offsets for next pixel sample
      _array1DOffset = _array2DOffset = 0;
      return ++CurrentPixelSampleIndex < _samplesPerPixel;
    }

    public abstract Sampler Clone(int seed);

    public virtual bool SetSampleNumber(int sampleNum)
    {
      // Reset array offsets for next pixel sample
      _array1DOffset = _array2DOffset = 0;
      CurrentPixelSampleIndex = sampleNum;
      return CurrentPixelSampleIndex < _samplesPerPixel;
    }

    public string StateString()
    {
      return "";
      // todo:
      //return "({PRId64},{currentPixel.X}), sample %" PRId64, currentPixel.x,
      //currentPixel.y, currentPixelSampleIndex);
    }
    long CurrentSampleNumber() { return CurrentPixelSampleIndex; }

    // Sampler Public Data
    protected readonly int _samplesPerPixel;

    // Sampler Protected Data
    protected Point2I currentPixel;
    protected int CurrentPixelSampleIndex;
    List<int> _samples1DArraySizes, _samples2DArraySizes;
    protected List<List<double>> _sampleArray1D;
    protected List<List<Point2D>> _sampleArray2D;

    // Sampler Private Data
    private int _array1DOffset, _array2DOffset;
  }
}
