using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Geometry;
using PbrtNet.Core.Interactions;

namespace PbrtNet.Core.Api
{
  public abstract class Camera
  {
    protected Camera(AnimatedTransform cameraToWorld, double shutterOpen, double shutterClose, Film film, Medium medium)
    {
      CameraToWorld = cameraToWorld;
      ShutterOpen = shutterOpen;
      ShutterClose = shutterClose;
      Film = film;
      Medium = medium;

      if (CameraToWorld.HasScale())
      {
        // todo:  
        //Warning(
        //  "Scaling detected in world-to-camera transformation!\n"
        //"The system has numerous assumptions, implicit and explicit,\n"
        //"that this transform will have no scale factors in it.\n"
        //"Proceed at your own risk; your image may have errors or\n"
        //"the system may crash as a result of this.");
      }
    }

    public abstract double GenerateRay(CameraSample sample, out Ray ray);

    public virtual double GenerateRayDifferential(CameraSample sample, out RayDifferential rd)
    {
      double wt = GenerateRay(sample, out Ray r);
      rd = new RayDifferential(r);
      if (wt == 0.0)
      {
        return 0.0;
      }

      // Find camera ray after shifting a fraction of a pixel in the $x$ direction
      double wtx = 0.0;
      foreach (double eps in new List<double> { 0.5, -0.5 })
      {
        CameraSample sshift = sample;
        sshift.FilmPoint = new Point2D(sshift.FilmPoint.X + eps, sshift.FilmPoint.Y);
        wtx = GenerateRay(sshift, out Ray rx);
        rd.RxOrigin = rd.Origin + (rx.Origin - rd.Origin) / eps;
        rd.RxDirection = rd.Direction + (rx.Direction - rd.Direction) / eps;
        if (wtx != 0.0)
        {
          break;
        }
      }
      if (wtx == 0.0)
      {
        return 0.0;
      }

      // Find camera ray after shifting a fraction of a pixel in the $y$ direction
      double wty = 0.0;
      foreach (double eps in new List<double> { 0.5, -0.5 })
      {
        CameraSample sshift = sample;
        sshift.FilmPoint = new Point2D(sshift.FilmPoint.X, sshift.FilmPoint.Y + eps);
        wty = GenerateRay(sshift, out Ray ry);
        rd.RyOrigin = rd.Origin + (ry.Origin - rd.Origin) / eps;
        rd.RyDirection = rd.Direction + (ry.Direction - rd.Direction) / eps;
        if (wty != 0.0)
        {
          break;
        }
      }
      if (wty == 0.0)
      {
        return 0.0;
      }

      rd.HasDifferentials = true;
      return wt;
    }

    // todo: make this abstract?
    public virtual Spectrum We(Ray ray, out Point2D pRaster2)
    {
      throw new NotImplementedException();
    }

    // todo: make this abstract?
    public virtual void Pdf_We(Ray ray, out double pdfPos, out double pdfDir)
    {
      throw new NotImplementedException();
    }

    // todo: make this abstract?
    public virtual Spectrum Sample_Wi(
      Interaction it,
      Point2D u,
      out Vector3D wi,
      out double df,
      out Point2D pRaster,
      out VisibilityTester vis)
    {
      throw new NotImplementedException();
    }

    // Camera Public Data
    public AnimatedTransform CameraToWorld { get; }
    public double ShutterOpen { get; }
    public double ShutterClose { get; }
    public Film Film { get; }
    public Medium Medium { get; }
  }
}
