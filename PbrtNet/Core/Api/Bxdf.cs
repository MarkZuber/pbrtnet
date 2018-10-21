// -----------------------------------------------------------------------
// <copyright file="Bxdf.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Api
{
  [Flags]
  public enum BxdfType
  {
    None = 0,
    Reflection = 1 << 0,
    Transmission = 1 << 1,
    Diffuse = 1 << 2,
    Glossy = 1 << 3,
    Specular = 1 << 4,
    All = Diffuse | Glossy | Specular | Reflection | Transmission,
  }

  public abstract class Bxdf
  {
    protected Bxdf(BxdfType type)
    {
      Type = type;
    }

    public BxdfType Type { get; }

    public bool MatchesFlags(BxdfType t)
    {
      return (Type & t) == Type;
    }

    public abstract Spectrum f(Vector3D wo, Vector3D wi);

    public virtual Spectrum Sample_f(
      Vector3D wo,
      out Vector3D wi,
      Point2D sample,
      out double pdf,
      out BxdfType sampledType)
    {
      sampledType = BxdfType.None;

      // Cosine-sample the hemisphere, flipping the direction if necessary
      wi = Sampling.CosineSampleHemisphere(sample);
      if (wo.Z < 0.0)
      {
        wi = new Vector3D(wi.X, wi.Y, -wi.Z);
      }

      pdf = Pdf(wo, wi);
      return f(wo, wi);
    }

    public virtual Spectrum rho(Vector3D wo, int nSamples, IEnumerable<Point2D> samples)
    {
      Spectrum r = Spectrum.Create(0.0);
      foreach (Point2D sample in samples)
      {
        // Estimate one term of $\rho_\roman{hd}$
        Spectrum f = Sample_f(wo, out Vector3D wi, sample, out double pdf, out BxdfType sampledType);
        if (pdf > 0)
        {
          r += f * Reflection.AbsCosTheta(wi) / pdf;
        }
      }
      return r / nSamples;

    }

    public virtual Spectrum rho(int nSamples, IEnumerable<Point2D> samples1, IEnumerable<Point2D> samples2)
    {
      var u1 = samples1.ToList();
      var u2 = samples2.ToList();
      Spectrum r = Spectrum.Create(0.0);
      for (int i = 0; i < nSamples; ++i)
      {
        // Estimate one term of $\rho_\roman{hh}$
        Vector3D wo = Sampling.UniformSampleHemisphere(u1[i]);
        double pdfo = Sampling.UniformHemispherePdf();
        Spectrum f = Sample_f(wo, out Vector3D wi, u2[i], out double pdfi, out BxdfType sampledType);
        if (pdfi > 0)
        {
          r += f * Reflection.AbsCosTheta(wi) * Reflection.AbsCosTheta(wo) / (pdfo * pdfi);
        }
      }
      return r / (Math.PI * nSamples);

    }

    public virtual double Pdf(Vector3D wo, Vector3D wi)
    {
      return Reflection.SameHemisphere(wo, wi) ? Reflection.AbsCosTheta(wi) * PbrtMath.InvPi : 0.0;
    }
  }
}