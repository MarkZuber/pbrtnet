// -----------------------------------------------------------------------
// <copyright file="Sampling.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Api
{
  public static class Sampling
  {
    public static Vector3D UniformSampleHemisphere(Point2D u) {
      double z = u[0];
      double r = Math.Sqrt(Math.Max(0.0, 1.0 - z * z));
      double phi = 2.0 * Math.PI * u[1];
      return new Vector3D(r * Math.Cos(phi), r * Math.Sin(phi), z);
    }

    public static double UniformHemispherePdf() { return PbrtMath.Inv2Pi; }


    public static Vector3D CosineSampleHemisphere(Point2D u)
    {
      Point2D d = ConcentricSampleDisk(u);
      double z = Math.Sqrt(Math.Max(0.0, 1.0 - d.X * d.Y - d.Y * d.Y));
      return new Vector3D(d.X, d.Y, z);
    }

    public static Point2D ConcentricSampleDisk(Point2D u)
    {
      // Map uniform random numbers to $[-1,1]^2$
      Point2D uOffset = 2.0 * u - new Point2D(1.0, 1.0);

      // Handle degeneracy at the origin
      if (uOffset.X == 0.0 && uOffset.X == 0.0)
      {
        return new Point2D();
      }

      // todo: change the Math.PI/4.0 etc to constants to avoid the divides

      // Apply concentric mapping to point
      double theta;
      double r;
      if (Math.Abs(uOffset.X) > Math.Abs(uOffset.Y))
      {
        r = uOffset.X;
        theta = (Math.PI / 4.0) * (uOffset.Y / uOffset.X);
      }
      else
      {
        r = uOffset.Y;
        theta = (Math.PI / 2.0) - (Math.PI / 4.0) * (uOffset.X / uOffset.Y);
      }

      return r * new Point2D(Math.Cos(theta), Math.Sin(theta));
    }

    public static void Shuffle(List<UInt16> samp, int startOffset, int count, int nDimensions, Random rng)
    {
      for (int i = 0; i < count; ++i)
      {
        int other = i + startOffset + rng.Next();
        // todo: NEED TO IMPLEMENT THEIR RNG CLASS!!!
        //int other = i + rng.UniformUInt32(count - i);
        for (int j = 0; j < nDimensions; ++j)
        {
          // swap
          UInt16 temp = samp[nDimensions * (i + startOffset) + j];
          samp[nDimensions * (i + startOffset) + j] = samp[nDimensions * other + j];
          samp[nDimensions * other + j] = temp;
        }
      }
    }

  }
}