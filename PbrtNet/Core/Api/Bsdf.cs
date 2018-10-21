// -----------------------------------------------------------------------
// <copyright file="Bsdf.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using PbrtNet.Core.Geometry;
using PbrtNet.Core.Interactions;

namespace PbrtNet.Core.Api
{
  public class Bsdf
  {
    private const int MaxBxDFs = 8;

    // BSDF Private Data
    private readonly Normal3D _ns;
    private readonly Normal3D _ng;

    private readonly Vector3D _ss;
    private readonly Vector3D _ts;

    readonly Bxdf[] _bxdfs = new Bxdf[MaxBxDFs];
    private int _nBxDFs = 0;

    // BSDF Public Methods
    public Bsdf(SurfaceInteraction si, double eta = 1.0)
    {
      Eta = eta;

      _ns = si.ShadingN;
      _ng = si.N;
      _ss = si.ShadingDpdu.Normalize();
      _ts = _ns.Cross(_ss);
    }

    public double Eta { get; }


  public void Add(Bxdf b)
    {
      // todo: CHECK_LT(_nBxDFs, MaxBxDFs);
      _bxdfs[_nBxDFs++] = b;
    }

    public int NumComponents(BxdfType flags = BxdfType.All)
    {
      int num = 0;
      for (int i = 0; i < _nBxDFs; ++i)
      {
        if (_bxdfs[i].MatchesFlags(flags))
        {
          ++num;
        }
      }

      return num;

    }

    public Vector3D WorldToLocal(Vector3D v)
    {
      return new Vector3D(v.Dot(_ss), v.Dot(_ts), v.Dot(_ns));
    }

    public Vector3D LocalToWorld(Vector3D v)
    {
      return new Vector3D(
        _ss.X * v.X + _ts.X * v.Y + _ns.X * v.Z,
        _ss.Y * v.X + _ts.Y * v.Y + _ns.Y * v.Z,
        _ss.Z * v.X + _ts.Z * v.Y + _ns.Z * v.Z);
    }

    public Spectrum f(Vector3D woW, Vector3D wiW, BxdfType flags = BxdfType.All)
    {
      // ProfilePhase pp(Prof::BSDFEvaluation);
      Vector3D wi = WorldToLocal(wiW), wo = WorldToLocal(woW);
      if (wo.Z == 0.0)
      {
        return Spectrum.Create(0.0);
      }

      bool reflect = wiW.Dot(_ng) * woW.Dot(_ng) > 0.0;
      Spectrum f = Spectrum.Create(0.0);
      for (int i = 0; i < _nBxDFs; ++i)
      {
        if (_bxdfs[i].MatchesFlags(flags) &&
            ((reflect && ((_bxdfs[i].Type & BxdfType.Reflection) == BxdfType.Reflection)) ||
             (!reflect && ((_bxdfs[i].Type & BxdfType.Transmission) == BxdfType.Transmission))))
        {
          f += _bxdfs[i].f(wo, wi);
        }
      }

      return f;
    }

    public Spectrum rho(
      int nSamples,
      IEnumerable<Point2D> samples1,
      IEnumerable<Point2D> samples2,
      BxdfType flags = BxdfType.All)
    {
      Spectrum ret = Spectrum.Create(0.0);
      for (int i = 0; i < _nBxDFs; ++i)
      {
        if (_bxdfs[i].MatchesFlags(flags))
        {
          ret += _bxdfs[i].rho(nSamples, samples1, samples2);
        }
      }

      return ret;

    }

    public Spectrum rho(Vector3D wo, int nSamples, IEnumerable<Point2D> samples, BxdfType flags = BxdfType.All)
    {
      Spectrum ret = Spectrum.Create(0.0);
      for (int i = 0; i < _nBxDFs; ++i)
      {
        if (_bxdfs[i].MatchesFlags(flags))
        {
          ret += _bxdfs[i].rho(wo, nSamples, samples);
        }
      }

      return ret;

    }

    public Spectrum Sample_f(
      Vector3D woWorld,
      out Vector3D wiWorld,
      Point2D u,
      out double pdf,
      out BxdfType sampledType,
      BxdfType type = BxdfType.All)
    {
      // ProfilePhase pp(Prof::BSDFSampling);
      // Choose which _BxDF_ to sample
      int matchingComps = NumComponents(type);
      if (matchingComps == 0)
      {
        pdf = 0.0;
        sampledType = BxdfType.None;
        wiWorld = new Vector3D();
        return Spectrum.Create(0.0);
      }

      int comp = Math.Min(Convert.ToInt32(Math.Floor(u[0] * matchingComps)), matchingComps - 1);

      // Get _BxDF_ pointer for chosen component
      Bxdf bxdf = null;
      int count = comp;
      for (int i = 0; i < _nBxDFs; ++i)
      {
        if (_bxdfs[i].MatchesFlags(type) && count-- == 0)
        {
          bxdf = _bxdfs[i];
          break;
        }
      }

      //CHECK(bxdf != nullptr);
      //VLOG(2) << "BSDF::Sample_f chose comp = " << comp << " / matching = " <<
      //    matchingComps << ", bxdf: " << bxdf->ToString();

      // Remap _BxDF_ sample _u_ to $[0,1)^2$
      Point2D uRemapped = new Point2D(Math.Min(u[0] * matchingComps -comp, PbrtMath.OneMinusEpsilon),
                      u[1]);

      // Sample chosen _BxDF_
      Vector3D wi, wo = WorldToLocal(woWorld);
      if (wo.Z == 0)
      {
        wiWorld = new Vector3D();
        pdf = 0.0;
        sampledType = BxdfType.None;
        return Spectrum.Create(0.0);
      }

      pdf = 0.0;
      sampledType = bxdf.Type;

      Spectrum f = bxdf.Sample_f(wo, out wi, uRemapped, out pdf, out sampledType);
      //VLOG(2) << "For wo = " << wo << ", sampled f = " << f << ", pdf = "
      //        << *pdf << ", ratio = " << ((*pdf > 0) ? (f / *pdf) : Spectrum(0.))
      //        << ", wi = " << wi;
      if (pdf == 0.0)
      {
        sampledType = BxdfType.None;
        wiWorld = new Vector3D();
        return Spectrum.Create(0.0);
      }
      wiWorld = LocalToWorld(wi);

      // Compute overall PDF with all matching _BxDF_s
      if ((bxdf.Type & BxdfType.Specular) != BxdfType.Specular && matchingComps > 1)
      {
        for (int i = 0; i < _nBxDFs; ++i)
        {
          if (_bxdfs[i] != bxdf && _bxdfs[i].MatchesFlags(type))
          {
            pdf += _bxdfs[i].Pdf(wo, wi);
          }
        }
      }

      if (matchingComps > 1)
      {
        pdf /= matchingComps;
      }

      // Compute value of BSDF for sampled direction
      if ((bxdf.Type & BxdfType.Specular) != BxdfType.Specular)
      {
        bool reflect = wiWorld.Dot(_ng) * woWorld.Dot(_ng) > 0.0;
        f = Spectrum.Create(0.0);
        for (int i = 0; i < _nBxDFs; ++i)
        {
          if (_bxdfs[i].MatchesFlags(type) &&
              ((reflect && ((_bxdfs[i].Type & BxdfType.Reflection) == BxdfType.Reflection) ||
               (!reflect && ((_bxdfs[i].Type & BxdfType.Transmission) == BxdfType.Transmission)))))
          {
            f += _bxdfs[i].f(wo, wi);
          }
        }
      }
      //VLOG(2) << "Overall f = " << f << ", pdf = " << *pdf << ", ratio = "
      //        << ((*pdf > 0) ? (f / *pdf) : Spectrum(0.));
      return f;
    }

    public double Pdf(Vector3D woWorld, Vector3D wiWorld, BxdfType flags = BxdfType.All)
    {
      //ProfilePhase pp(Prof::BSDFPdf);
      if (_nBxDFs == 0)
      {
        return 0.0;
      }

      Vector3D wo = WorldToLocal(woWorld);
      Vector3D wi = WorldToLocal(wiWorld);
      if (wo.Z == 0.0)
      {
        return 0.0;
      }

      double pdf = 0.0;
      int matchingComps = 0;
      for (int i = 0; i < _nBxDFs; ++i)
      {
        if (_bxdfs[i].MatchesFlags(flags))
        {
          ++matchingComps;
          pdf += _bxdfs[i].Pdf(wo, wi);
        }
      }

      double v = matchingComps > 0 ? pdf / matchingComps : 0.0;
      return v;
    }

    public override string ToString()
    {
      string s = $"[ BSDF eta: {Eta} nBxDFs: {_nBxDFs}";
      for (int i = 0; i < _nBxDFs; ++i)
      {
        s += $"\n bxdfs[{i}]: {_bxdfs[i].ToString()}";
      }

      s += "]";
      return s;
    }
  }
}