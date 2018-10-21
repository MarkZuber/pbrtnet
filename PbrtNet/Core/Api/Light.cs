// -----------------------------------------------------------------------
// <copyright file="Light.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System;
using PbrtNet.Core.Geometry;
using PbrtNet.Core.Interactions;

namespace PbrtNet.Core.Api
{
  [Flags]
  public enum LightFlags
  {
    DeltaPosition = 1,
    DeltaDirection = 2,
    Area = 4,
    Infinite = 8
  }

  public abstract class Light
  {
    protected Light(LightFlags flags, Transform lightToWorld, MediumInterface mediumInterface, int nSamples)
    {
      Flags = flags;
      NumSamples = nSamples;
      MediumInterface = mediumInterface;
      LightToWorld = lightToWorld;
      WorldToLight = lightToWorld.Inverse();
    }

    public LightFlags Flags { get; }
    public int NumSamples { get; }
    public MediumInterface MediumInterface { get; }

    protected Transform LightToWorld { get; }
    protected Transform WorldToLight { get; }

    public abstract Spectrum Sample_Li(Interaction it, Point2D u, out Vector3D wi, out double pdf, out VisibilityTester vis);

    public abstract Spectrum Power();

    public void Preprocess(Scene scene)
    {
    }

    public virtual Spectrum Le(RayDifferential r)
    {
      return Spectrum.Create(0.0);
    }

    public abstract double Pdf_Li(Interaction it, Vector3D wi);

    public abstract Spectrum Sample_Le(
      Point2D u1,
      Point2D u2,
      double time,
      out Ray ray,
      out Normal3D nLight,
      out double pdfPos,
      out double pdfDir);

    public abstract void Pdf_Le(Ray ray, Normal3D nLight, out double pdfPos, out double pdfDir);
  }
}