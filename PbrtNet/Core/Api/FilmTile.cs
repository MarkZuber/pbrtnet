// -----------------------------------------------------------------------
// <copyright file="FilmTile.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Api
{
  public class FilmTilePixel
  {
    public Spectrum ContribSum { get; set; } = Spectrum.Create(0.0);
    public double FilterWeightSum { get; set; } = 0.0;
  };

  public class FilmTile
  {
    private readonly List<double> _filterTable;
    private readonly List<FilmTilePixel> _pixels;

    public FilmTile(
      Bounds2I pixelBounds,
      Vector2D filterRadius,
      IEnumerable<double> filterTable,
      int filterTableSize, 
      double maxSampleLuminance)
    {
      PixelBounds = pixelBounds;
      FilterRadius = filterRadius;
      InvFilterRadius = new Vector2D(1.0 / filterRadius.X, 1.0 / filterRadius.Y);
      _filterTable = filterTable.ToList();
      FilterTableSize = filterTableSize;
      MaxSampleLuminance = maxSampleLuminance;
      _pixels = new List<FilmTilePixel>(Math.Max(0, pixelBounds.Area));
    }

    public Bounds2I PixelBounds { get; }
    public Vector2D FilterRadius { get; }
    public Vector2D InvFilterRadius { get; }
    public int FilterTableSize { get; }
  
    public double MaxSampleLuminance { get; }

    public void AddSample(Point2D pFilm, Spectrum L, double sampleWeight = 1.0)
    {
      //ProfilePhase _(Prof::AddFilmSample);
      if (L.Y() > MaxSampleLuminance)
      {
        L *= MaxSampleLuminance / L.Y();
      }

      // Compute sample's raster bounds
      Point2D pFilmDiscrete = pFilm - new Point2D(0.5, 0.5);
      Point2I p0 = (pFilmDiscrete - FilterRadius.ToPoint2D()).Ceiling().ToPoint2I();
      Point2I p1 = (pFilmDiscrete + FilterRadius.ToPoint2D()).Floor().ToPoint2I() + new Point2I(1, 1);
      p0 = Point2I.Max(p0, PixelBounds.MinPoint);
      p1 = Point2I.Min(p1, PixelBounds.MaxPoint);

      // Loop over filter support and add sample to pixel arrays

      // Precompute $x$ and $y$ filter table offsets
      int[] ifx = new int[p1.X - p0.X];
      for (int x = p0.X; x < p1.X; ++x)
      {
        double fx = Math.Abs((x - pFilmDiscrete.X) * InvFilterRadius.X * FilterTableSize);
        ifx[x - p0.X] = Math.Min((int)Math.Floor(fx), FilterTableSize - 1);
      }

      int[] ify = new int[p1.Y - p0.Y];
      for (int y = p0.Y; y < p1.Y; ++y)
      {
        double fy = Math.Abs((y - pFilmDiscrete.Y) * InvFilterRadius.Y * FilterTableSize);
        ify[y - p0.Y] = Math.Min((int)Math.Floor(fy), FilterTableSize - 1);
      }

      for (int y = p0.Y; y < p1.Y; ++y)
      {
        for (int x = p0.X; x < p1.X; ++x)
        {
          // Evaluate filter value at $(x,y)$ pixel
          int offset = ify[y - p0.Y] * FilterTableSize + ifx[x - p0.X];
          double filterWeight = _filterTable[offset];

          // Update pixel values with filtered sample contribution
          FilmTilePixel pixel = GetPixel(new Point2I(x, y));
          pixel.ContribSum += L * sampleWeight * filterWeight;
          pixel.FilterWeightSum += filterWeight;
        }
      }
    }

    public FilmTilePixel GetPixel(Point2I p)
    {
      //CHECK(InsideExclusive(p, pixelBounds));
      int width = PixelBounds.MaxPoint.X - PixelBounds.MinPoint.X;
      int offset = (p.X - PixelBounds.MinPoint.X) + (p.Y - PixelBounds.MinPoint.Y) * width;
      return _pixels[offset];
    }
  }
}