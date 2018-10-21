using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Api;
using PbrtNet.Core.Geometry;
using PbrtNet.Core.Interactions;

namespace PbrtNet.Core.Cameras
{
  public class PerspectiveCamera : ProjectiveCamera
  {
    private readonly Vector3D _dxCamera;
    private readonly Vector3D _dyCamera;
    private readonly double _a;

    /// <inheritdoc />
    public PerspectiveCamera(AnimatedTransform cameraToWorld, Bounds2D screenWindow, double shutterOpen, double shutterClose, double lensr, double focald, double fov, Film film, Medium medium)
      : base(cameraToWorld, Transform.Perspective(fov, 1e-2f, 1000.0), screenWindow, shutterOpen, shutterClose, lensr, focald, film, medium)
    {
      // Compute differential changes in origin for perspective camera rays
      _dxCamera =
        (RasterToCamera.AtPoint(new Point3D(1.0, 0.0, 0.0)) - RasterToCamera.AtPoint(new Point3D(0.0, 0.0, 0.0))).ToVector3D();
      _dyCamera =
        (RasterToCamera.AtPoint(new Point3D(0.0, 1.0, 0.0)) - RasterToCamera.AtPoint(new Point3D(0.0, 0.0, 0.0))).ToVector3D();

      // Compute image plane bounds at $z=1$ for _PerspectiveCamera_
      Point2I res = film.FullResolution;
      Point3D MinPoint = RasterToCamera.AtPoint(new Point3D(0.0, 0.0, 0.0));
      Point3D MaxPoint = RasterToCamera.AtPoint(new Point3D(res.X, res.Y, 0.0));
      MinPoint /= MinPoint.Z;
      MaxPoint /= MaxPoint.Z;
      _a = Math.Abs((MaxPoint.X - MinPoint.X) * (MaxPoint.Y - MinPoint.Y));
    }

    /// <inheritdoc />
    public override double GenerateRay(CameraSample sample, out Ray ray)
    {
      //ProfilePhase prof(Prof::GenerateCameraRay);
      // Compute raster and camera sample positions
      Point3D pFilm = new Point3D(sample.FilmPoint.X, sample.FilmPoint.Y, 0.0);
      Point3D pCamera = RasterToCamera.AtPoint(pFilm);
      ray = new Ray(new Point3D(0.0, 0.0, 0.0), pCamera.ToVector3D().Normalize());
      // Modify ray for depth of field
      if (LensRadius > 0.0)
      {
        // Sample point on lens
        Point2D pLens = LensRadius * Sampling.ConcentricSampleDisk(sample.LensPoint);

        // Compute point on plane of focus
        double ft = FocalDistance / ray.Direction.Z;
        Point3D pFocus = ray.AtPoint(ft);

        // Update ray for effect of lens
        ray.Origin = new Point3D(pLens.X, pLens.Y, 0.0);
        ray.Direction = (pFocus - ray.Origin).ToVector3D().Normalize();
      }
      ray.Time = PbrtMath.Lerp(sample.Time, ShutterOpen, ShutterClose);
      ray.Medium = Medium;
      ray = CameraToWorld.ExecuteTransform(ray);
      return 1.0;
    }

    /// <inheritdoc />
    public override double GenerateRayDifferential(CameraSample sample, out RayDifferential ray)
    {
      //ProfilePhase prof(Prof::GenerateCameraRay);
      // Compute raster and camera sample positions
      Point3D pFilm = new Point3D(sample.FilmPoint.X, sample.FilmPoint.Y, 0.0);
      Point3D pCamera = RasterToCamera.ExecuteTransform(pFilm);
      Vector3D dir = new Vector3D(pCamera.X, pCamera.Y, pCamera.Z).Normalize();
      ray = new RayDifferential(new Point3D(0.0, 0.0, 0.0), dir);
      // Modify ray for depth of field
      if (LensRadius > 0.0)
      {
        // Sample point on lens
        Point2D pLens = LensRadius * Sampling.ConcentricSampleDisk(sample.LensPoint);

        // Compute point on plane of focus
        double ft = FocalDistance / ray.Direction.Z;
        Point3D pFocus = ray.AtPoint(ft);

        // Update ray for effect of lens
        ray.Origin = new Point3D(pLens.X, pLens.Y, 0.0);
        ray.Direction = (pFocus - ray.Origin).ToVector3D().Normalize();
      }

      // Compute offset rays for _PerspectiveCamera_ ray differentials
      if (LensRadius > 0.0)
      {
        // Compute _PerspectiveCamera_ ray differentials accounting for lens

        // Sample point on lens
        Point2D pLens = LensRadius * Sampling.ConcentricSampleDisk(sample.LensPoint);
        Vector3D dx = (pCamera.ToVector3D() + _dxCamera).Normalize();
        double ft = FocalDistance / dx.Z;
        Point3D pFocus = new Point3D(0.0, 0.0, 0.0) + (ft * dx).ToPoint3D();
        ray.RxOrigin = new Point3D(pLens.X, pLens.Y, 0.0);
        ray.RxDirection = (pFocus - ray.RxOrigin).ToVector3D().Normalize();

        Vector3D dy = (pCamera.ToVector3D() + _dyCamera).Normalize();
        ft = FocalDistance / dy.Z;
        pFocus = new Point3D(0.0, 0.0, 0.0) + (ft * dy).ToPoint3D();
        ray.RyOrigin = new Point3D(pLens.X, pLens.Y, 0.0);
        ray.RyDirection = (pFocus - ray.RyOrigin).ToVector3D().Normalize();
      }
      else
      {
        ray.RxOrigin = ray.RyOrigin = ray.Origin;
        ray.RxDirection = (pCamera.ToVector3D() + _dxCamera).Normalize();
        ray.RyDirection = (pCamera.ToVector3D() + _dyCamera).Normalize();
      }
      ray.Time = PbrtMath.Lerp(sample.Time, ShutterOpen, ShutterClose);
      ray.Medium = Medium;
      ray = new RayDifferential(CameraToWorld.ExecuteTransform(ray))
      {
        HasDifferentials = true
      };
      return 1.0;
    }

    /// <inheritdoc />
    public override Spectrum We(Ray ray, out Point2D pRaster2)
    {
      // Interpolate camera matrix and check if $\w{}$ is forward-facing
      CameraToWorld.Interpolate(ray.Time, out Transform c2w);
      double cosTheta = ray.Direction.Dot(c2w.ExecuteTransform(new Vector3D(0.0, 0.0, 1.0)));
      if (cosTheta <= 0.0)
      {
        pRaster2 = null;
        return Spectrum.Create(0.0);
      }

      // Map ray $(\p{}, \w{})$ onto the raster grid
      Point3D pFocus = ray.AtPoint((LensRadius > 0.0 ? FocalDistance : 1.0) / cosTheta);
      Point3D pRaster = RasterToCamera.ExecuteTransform(c2w.Inverse().ExecuteTransform(pFocus));
      //Point3D pRaster = Inverse(RasterToCamera)(Inverse(c2w.Inverse)(pFocus));

      // Return raster position if requested
        pRaster2 = new Point2D(pRaster.X, pRaster.Y);

      // Return zero importance for out of bounds points
      Bounds2I sampleBounds = Film.GetSampleBounds();
      if (pRaster.X < sampleBounds.MinPoint.X || pRaster.Z >= sampleBounds.MaxPoint.X ||
          pRaster.Y < sampleBounds.MinPoint.Y || pRaster.Y >= sampleBounds.MaxPoint.Y)
      {
        return Spectrum.Create(0.0);
      }

      // Compute lens area of perspective camera
      double lensArea = LensRadius != 0.0 ? (Math.PI * LensRadius * LensRadius) : 1.0;

      // Return importance for point on image plane
      double cos2Theta = cosTheta * cosTheta;
      return Spectrum.Create(1.0 / (_a * lensArea * cos2Theta * cos2Theta));
    }

    /// <inheritdoc />
    public override void Pdf_We(Ray ray, out double pdfPos, out double pdfDir)
    {
      // Interpolate camera matrix and fail if $\w{}$ is not forward-facing
      CameraToWorld.Interpolate(ray.Time, out Transform c2w);
      double cosTheta = ray.Direction.Dot(c2w.ExecuteTransform(new Vector3D(0.0, 0.0, 1.0)));
      if (cosTheta <= 0)
      {
        pdfPos = pdfDir = 0.0;
        return;
      }

      // Map ray $(\p{}, \w{})$ onto the raster grid
      Point3D pFocus = ray.AtPoint((LensRadius > 0.0 ? FocalDistance : 1.0) / cosTheta);
      Point3D pRaster = RasterToCamera.Inverse().ExecuteTransform(c2w.Inverse().ExecuteTransform(pFocus));

      // Return zero probability for out of bounds points
      Bounds2I sampleBounds = Film.GetSampleBounds();
      if (pRaster.X < sampleBounds.MinPoint.X || pRaster.X >= sampleBounds.MaxPoint.X ||
          pRaster.Y < sampleBounds.MinPoint.Y || pRaster.Y >= sampleBounds.MaxPoint.Y)
      {
        pdfPos = pdfDir = 0.0;
        return;
      }

      // Compute lens area of perspective camera
      double lensArea = LensRadius != 0.0 ? (Math.PI * LensRadius * LensRadius) : 1.0;
      pdfPos = 1.0 / lensArea;
      pdfDir = 1.0 / (_a * cosTheta * cosTheta * cosTheta);
    }

    /// <inheritdoc />
    public override Spectrum Sample_Wi(
      Interaction it,
      Point2D u,
      out Vector3D wi,
      out double pdf,
      out Point2D pRaster,
      out VisibilityTester vis)
    {
      // Uniformly sample a lens interaction _lensIntr_
      Point2D pLens = LensRadius * Sampling.ConcentricSampleDisk(u);
      Point3D pLensWorld = CameraToWorld.ExecuteTransform(it.Time, new Point3D(pLens.X, pLens.Y, 0.0));
      Interaction lensIntr = new Interaction(pLensWorld, it.Time, new MediumInterface(Medium));
      lensIntr.N = (CameraToWorld.ExecuteTransform(it.Time, new Point3D(0.0, 0.0, 1.0))).ToVector3D().ToNormal3D();

      // Populate arguments and compute the importance value
      vis = new VisibilityTester(it, lensIntr);
      wi = (lensIntr.P - it.P).ToVector3D();
      double dist = wi.Length();
      wi /= dist;

      // Compute PDF for importance arriving at _ref_

      // Compute lens area of perspective camera
      double lensArea = LensRadius != 0.0 ? (Math.PI * LensRadius * LensRadius) : 1.0;
      pdf = (dist * dist) / (lensIntr.N.AbsDot(wi) * lensArea);
      return We(lensIntr.SpawnRay(-wi), out pRaster);
    }

    public static Camera Create(ParamSet paramSet, AnimatedTransform cam2World, Film film, Medium medium)
    {
      // Extract common camera parameters from _ParamSet_
      double shutteropen = paramSet.FindOneFloat("shutteropen", 0.0);
      double shutterclose = paramSet.FindOneFloat("shutterclose", 1.0);
      if (shutterclose < shutteropen)
      {
        //Warning("Shutter close time [%f] < shutter open [%f].  Swapping them.",
        //        shutterclose, shutteropen);

        PbrtMath.Swap(ref shutterclose, ref shutteropen);
      }
      double lensradius = paramSet.FindOneFloat("lensradius", 0.0);
      double focaldistance = paramSet.FindOneFloat("focaldistance", 1e6);
      double frame = paramSet.FindOneFloat(
        "frameaspectratio",
        Convert.ToDouble(film.FullResolution.X) / Convert.ToDouble(film.FullResolution.Y));
      Bounds2D screen;
      if (frame > 1.0)
      {
        screen = new Bounds2D(new Point2D(-frame, -1.0), new Point2D(frame, 1.0));
      }
      else
      {
        screen = new Bounds2D(new Point2D(-1.0, -1.0 /frame), new Point2D(1.0, 1.0/frame));
      }
      double[] sw =  paramSet.FindFloat("screenwindow");
      if (sw != null)
      {
        if (sw.Length == 4)
        {
          screen = new Bounds2D(new Point2D(sw[0], sw[2]), new Point2D(sw[1], sw[3]));
        }
        else
        {
          //Error("\"screenwindow\" should have four values");
        }
      }
      double fov = paramSet.FindOneFloat("fov", 90.0);
      double halffov = paramSet.FindOneFloat("halffov", -1.0);
      if (halffov > 0.0)
      {
        // hack for structure synth, which exports half of the full fov
        fov = 2.0 * halffov;
      }

      return new PerspectiveCamera(cam2World, screen, shutteropen, shutterclose,
                                   lensradius, focaldistance, fov, film, medium);
    }
  }
}
