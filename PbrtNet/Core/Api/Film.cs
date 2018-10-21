// -----------------------------------------------------------------------
// <copyright file="Film.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Api
{
  public class Film
  {
    private const int FilterTableWidth = 16;

    private readonly object _lockObject = new object();
    private readonly double _maxSampleLuminance;

    private readonly Pixel[] _pixels;

    // std::mutex mutex;
    private readonly double _scale;
    private readonly double[] FilterTable = new double[FilterTableWidth * FilterTableWidth];

    public Film(
      Point2I resolution,
      Bounds2D cropWindow,
      Filter filter,
      double diagonal,
      string filename,
      double scale,
      double maxSampleLuminance = double.MaxValue)
    {
      FullResolution = resolution;
      Diagonal = diagonal * 0.001;
      Filter = filter;
      Filename = filename;
      _scale = scale;
      _maxSampleLuminance = maxSampleLuminance;

      CroppedPixelBounds = new Bounds2I(
        new Point2I(
          Convert.ToInt32(Math.Ceiling(Convert.ToDouble(FullResolution.X) * cropWindow.MinPoint.X)),
          Convert.ToInt32(Math.Ceiling(Convert.ToDouble(FullResolution.Y) * cropWindow.MinPoint.Y))),
        new Point2I(
          Convert.ToInt32(Math.Ceiling(Convert.ToDouble(FullResolution.X) * cropWindow.MaxPoint.X)),
          Convert.ToInt32(Math.Ceiling(Convert.ToDouble(FullResolution.Y) * cropWindow.MaxPoint.Y))));
      //LOG(INFO) << "Created film with full resolution " << resolution <<
      //  ". Crop window of " << cropWindow << " -> croppedPixelBounds " <<
      //  croppedPixelBounds;

      // Allocate film image storage
      _pixels = new Pixel[CroppedPixelBounds.Area];
      // todo: this is a stat... _filmPixelMemory += croppedPixelBounds.Area() * sizeof(Pixel);

      // Precompute filter weight table
      int offset = 0;
      for (int y = 0; y < FilterTableWidth; ++y)
      {
        for (int x = 0; x < FilterTableWidth; ++x, ++offset)
        {
          Point2D p = new Point2D(
            (x + 0.5f) * filter.Radius.X / Convert.ToDouble(FilterTableWidth),
            (y + 0.5f) * filter.Radius.Y / Convert.ToDouble(FilterTableWidth));
          FilterTable[offset] = filter.Evaluate(p);
        }
      }
    }

    // Film Public Data
    public Point2I FullResolution { get; }
    public double Diagonal { get; }
    public Filter Filter { get; }
    public string Filename { get; }
    public Bounds2I CroppedPixelBounds { get; }

    public Bounds2I GetSampleBounds()
    {
      Bounds2D floatBounds = new Bounds2D(
        (CroppedPixelBounds.MinPoint.ToPoint2D() + new Point2D(0.5f, 0.5f) - Filter.Radius.ToPoint2D()).Floor(),
        (CroppedPixelBounds.MaxPoint.ToPoint2D() - new Point2D(0.5f, 0.5f) + Filter.Radius.ToPoint2D()).Ceiling());
      return floatBounds.ToBounds2I();
    }

    public Bounds2D GetPhysicalExtent()
    {
      double aspect = Convert.ToDouble(FullResolution.Y) / Convert.ToDouble(FullResolution.X);
      double x = Math.Sqrt(Diagonal * Diagonal / (1.0 + aspect * aspect));
      double y = aspect * x;
      return new Bounds2D(new Point2D(-x / 2.0, -y / 2.0), new Point2D(x / 2.0, y / 2.0));
    }

    public FilmTile GetFilmTile(Bounds2I sampleBounds)
    {
      // Bound image pixels that samples in _sampleBounds_ contribute to
      Point2D halfPixel = new Point2D(0.5, 0.5);
      Bounds2D floatBounds = sampleBounds.ToBounds2D();
      Point2I p0 = (floatBounds.MinPoint - halfPixel - Filter.Radius.ToPoint2D()).Ceiling().ToPoint2I();
      Point2I p1 = (floatBounds.MaxPoint - halfPixel + Filter.Radius.ToPoint2D()).Floor().ToPoint2I() +
                   new Point2I(1, 1);
      Bounds2I tilePixelBounds = new Bounds2I(p0, p1).Intersect(CroppedPixelBounds);
      return new FilmTile(tilePixelBounds, Filter.Radius, FilterTable, FilterTableWidth, _maxSampleLuminance);
    }

    public void MergeFilmTile(FilmTile tile)
    {
      //ProfilePhase p(Prof::MergeFilmTile);
      //VLOG(1) << "Merging film tile " << tile->pixelBounds;
      //std::lock_guard < std::mutex > lock (mutex) ;
      lock (_lockObject)
      {
        foreach (Point2I pixel in tile.PixelBounds.GetPoints())
        {
          // Merge _pixel_ into _Film::pixels_
          FilmTilePixel tilePixel = tile.GetPixel(pixel);
          Pixel mergePixel = GetPixel(pixel);
          double[] xyz = tilePixel.ContribSum.ToXyz();
          for (int i = 0; i < 3; ++i)
          {
            mergePixel.xyz[i] += xyz[i];
          }

          mergePixel.filterWeightSum += tilePixel.FilterWeightSum;
        }
      }
    }

    public void SetImage(Spectrum[] img)
    {
      int nPixels = CroppedPixelBounds.Area;
      for (int i = 0; i < nPixels; ++i)
      {
        Pixel p = _pixels[i];
        p.xyz = img[i].ToXyz();
        p.filterWeightSum = 1;
        p.splatXYZ[0] = p.splatXYZ[1] = p.splatXYZ[2] = 0.0;
      }
    }

    public void AddSplat(Point2D p, Spectrum v)
    {
      //ProfilePhase pp(Prof::SplatFilm);

      if (v.HasNaNs())
      {
        //LOG(ERROR) << StringPrintf("Ignoring splatted spectrum with NaN values "
        //"at (%f, %f)", p.x, p.y);
        return;
      }
      else if (v.Y() < 0.0)
      {
        //LOG(ERROR) << StringPrintf("Ignoring splatted spectrum with negative "
        //"luminance %f at (%f, %f)", v.y(), p.x, p.y);
        return;
      }
      else if (double.IsInfinity(v.Y()))
      {
        //LOG(ERROR) << StringPrintf("Ignoring splatted spectrum with infinite "
        //"luminance at (%f, %f)", p.x, p.y);
        return;
      }

      if (!p.ToPoint2I().InsideExclusive(CroppedPixelBounds))
      {
        return;
      }

      if (v.Y() > _maxSampleLuminance)
      {
        v *= _maxSampleLuminance / v.Y();
      }

      double[] xyz = v.ToXyz();
      Pixel pixel = GetPixel(p.ToPoint2I());
      for (int i = 0; i < 3; ++i)
      {
        pixel.splatXYZ[i] += xyz[i];
      }
    }

    public void WriteImage(double splatScale = 1.0)
    {
      // Convert image to RGB and compute final pixel values
      //LOG(INFO) <<
      //  "Converting image to RGB and computing final weighted pixel values";
      double[] rgb = new double[3 * CroppedPixelBounds.Area];
      int offset = 0;
      foreach (Point2I p in CroppedPixelBounds.GetPoints())
      {
        // Convert pixel XYZ color to RGB
        Pixel pixel = GetPixel(p);
        Array.Copy(Spectrum.XyzToRgb(pixel.xyz), 0, rgb, 3 * offset, 3);

        // Normalize pixel with weight sum
        double filterWeightSum = pixel.filterWeightSum;
        if (filterWeightSum != 0.0)
        {
          double invWt = 1.0 / filterWeightSum;
          rgb[3 * offset] = Math.Max(0.0, rgb[3 * offset] * invWt);
          rgb[3 * offset + 1] = Math.Max(0.0, rgb[3 * offset + 1] * invWt);
          rgb[3 * offset + 2] = Math.Max(0.0, rgb[3 * offset + 2] * invWt);
        }

        // Add splat value at pixel
        double[] splatXYZ = new double[3]
        {
          pixel.splatXYZ[0],
          pixel.splatXYZ[1],
          pixel.splatXYZ[2]
        };

        double[] splatRGB = Spectrum.XyzToRgb(splatXYZ);
        rgb[3 * offset] += splatScale * splatRGB[0];
        rgb[3 * offset + 1] += splatScale * splatRGB[1];
        rgb[3 * offset + 2] += splatScale * splatRGB[2];

        // Scale pixel value by _scale_
        rgb[3 * offset] *= _scale;
        rgb[3 * offset + 1] *= _scale;
        rgb[3 * offset + 2] *= _scale;
        ++offset;
      }

      // Write RGB image
      //LOG(INFO) << "Writing image " << filename << " with bounds " <<
      //  croppedPixelBounds;
      ImageIo.WriteImage(Filename, rgb[0], CroppedPixelBounds, FullResolution);
    }

    public void Clear()
    {
      foreach (Point2I p in CroppedPixelBounds.GetPoints())
      {
        Pixel pixel = GetPixel(p);
        for (int c = 0; c < 3; ++c)
        {
          pixel.splatXYZ[c] = pixel.xyz[c] = 0;
        }

        pixel.filterWeightSum = 0;
      }
    }

    // Film Private Methods
    private Pixel GetPixel(Point2I p)
    {
      // CHECK(InsideExclusive(p, croppedPixelBounds));
      int width = CroppedPixelBounds.MaxPoint.X - CroppedPixelBounds.MinPoint.X;
      int offset = (p.X - CroppedPixelBounds.MinPoint.X) + (p.Y - CroppedPixelBounds.MinPoint.Y) * width;
      return _pixels[offset];
    }

    // Film Private Data
    private class Pixel
    {
      public readonly double[] splatXYZ = new double[3]; // todo: was AtomicFloat
      public double filterWeightSum;
      public double pad = 0.0;
      public double[] xyz = new double[3];

      public Pixel()
      {
        xyz[0] = xyz[1] = xyz[2] = filterWeightSum = 0.0;
      }
    }

    public static Film Create(PbrtOptions options, ParamSet paramSet, Filter filter)
    {
      string filename;
      if (options.ImageFile != "")
      {
        filename = options.ImageFile;
        string paramsFilename = paramSet.FindOneString("filename", "");
        if (paramsFilename != "")
        {
          //Warning(
          //  "Output filename supplied on command line, \"%s\" is overriding filename provided in scene description file, \"%s\".",
          //PbrtOptions.imageFile.c_str(), paramsFilename.c_str());
        }
      }
      else
      {
        filename = paramSet.FindOneString("filename", "pbrt.exr");
      }

      int xres = paramSet.FindOneInt("xresolution", 1280);
      int yres = paramSet.FindOneInt("yresolution", 720);
      if (options.QuickRender)
      {
        xres = Math.Max(1, xres / 4);
      }

      if (options.QuickRender)
      {
        yres = Math.Max(1, yres / 4);
      }

      Bounds2D crop;
      int cwi;
      double[] cr = paramSet.FindFloat("cropwindow");
      if (cr != null && cr.Length == 4)
      {
        double minx = PbrtMath.Clamp(Math.Min(cr[0], cr[1]), 0.0, 1.0);
        double maxx = PbrtMath.Clamp(Math.Max(cr[0], cr[1]), 0.0, 1.0);
        double miny = PbrtMath.Clamp(Math.Min(cr[2], cr[3]), 0.0, 1.0);
        double maxy = PbrtMath.Clamp(Math.Max(cr[2], cr[3]), 0.0, 1.0);
        crop = new Bounds2D(new Point2D(minx, miny), new Point2D(maxx, maxy));
      }
      else if (cr != null)
      {
        throw new InvalidOperationException();
        //Error("%d values supplied for \"cropwindow\". Expected 4.", cwi);
      }
      else
      {
        crop = new Bounds2D(new Point2D(PbrtMath.Clamp(options.CropWindow[0,0], 0.0, 1.0),
                                        PbrtMath.Clamp(options.CropWindow[1,0], 0.0, 1.0)),
                        new Point2D(PbrtMath.Clamp(options.CropWindow[0,1], 0.0, 1.0),
                                PbrtMath.Clamp(options.CropWindow[1,1], 0.0, 1.0)));
      }

      double scale = paramSet.FindOneFloat("scale", 1.0);
      double diagonal = paramSet.FindOneFloat("diagonal", 35.0);
      double maxSampleLuminance = paramSet.FindOneFloat("maxsampleluminance",
                                                     double.PositiveInfinity);
      return new Film(new Point2I(xres, yres), crop, filter, diagonal,
                      filename, scale, maxSampleLuminance);

    }
  }
}