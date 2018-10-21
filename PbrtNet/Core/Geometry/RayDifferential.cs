using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Api;

namespace PbrtNet.Core.Geometry
{
  public class RayDifferential : Ray
  {
    public RayDifferential()
    {
      HasDifferentials = false;
    }

    public RayDifferential(
      Point3D origin,
      Vector3D direction,
      double tMax = double.MaxValue,
      double time = 0.0,
      Medium medium = null)
      : base(origin, direction, tMax, time, medium)
    {
      HasDifferentials = false;
    }

    public RayDifferential(Ray ray) : base(ray)
    {
      HasDifferentials = false;
    }

    public bool HasDifferentials { get;  set; }
    public Point3D RxOrigin { get;  set; }
    public Point3D RyOrigin { get;  set; }
    public Vector3D RxDirection { get;  set; }
    public Vector3D RyDirection { get;  set; }

    public void ScaleDifferentials(double s)
    {
      RxOrigin = Origin + (RxOrigin - Origin) * s;
      RyOrigin = Origin + (RyOrigin - Origin) * s;
      RxDirection = Direction + (RxDirection - Direction) * s;
      RyDirection = Direction + (RyDirection - Direction) * s;
    }
  }
}
