using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Api
{
  public static class Reflection
  {
    public static double AbsCosTheta(Vector3D w)
    {
      return Math.Abs(w.Z);
    }

    public static bool SameHemisphere(Vector3D w, Vector3D wp)
    {
      return w.Z * wp.Z > 0.0;
    }
  }
}