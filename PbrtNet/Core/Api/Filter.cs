using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Api
{
  public abstract class Filter
  {
    protected Filter(Vector2D radius)
    {
      Radius = radius;
      InvRadius = new Vector2D(1.0 / radius.X, 1.0 / radius.Y);
    }

    public Vector2D Radius { get; }
    public Vector2D InvRadius { get; }

    public abstract double Evaluate(Point2D p);
  }
}
