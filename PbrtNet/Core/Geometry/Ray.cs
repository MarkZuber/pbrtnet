using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Api;

namespace PbrtNet.Core.Geometry
{
  public class Ray
  {
    public Ray()
    {
      TMax = double.MaxValue;  // todo: this is Infinity in the book
      Medium = null;
      Time = 0.0;
      Origin = new Point3D();
      Direction = new Vector3D();
    }

    public Ray(
      Point3D origin,
      Vector3D direction,
      double tMax = double.MaxValue,
      double time = 0.0,
      Medium medium = null)
    {
      Origin = origin;
      Direction = direction;
      TMax = tMax;
      Time = time;
      Medium = medium;
    }

    public Ray(Ray r)
    {
      Origin = r.Origin;
      Direction = r.Direction;
      TMax = r.TMax;
      Time = r.Time;
      Medium = r.Medium;
    }

    public Point3D AtPoint(double t)
    {
      return Origin + Direction.ToPoint3D() * t;
    }

    public double TMax { get; set; }   
    public Point3D Origin { get; set; }
    public Vector3D Direction { get; set; }
    public double Time { get; set; }
    public Medium Medium { get; set; }
  }
}
