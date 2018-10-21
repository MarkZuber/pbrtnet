using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Api;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Filters
{
  public class BoxFilter : Filter
  {
    /// <inheritdoc />
    public BoxFilter(Vector2D radius)
      : base(radius)
    {
    }

    /// <inheritdoc />
    public override double Evaluate(Point2D p)
    {
      return 1.0;
    }

    public static BoxFilter Create(ParamSet paramSet)
    {
      double xw = paramSet.FindOneFloat("xwidth", 0.5);
      double yw = paramSet.FindOneFloat("ywidth", 0.5);
      return new BoxFilter(new Vector2D(xw, yw));
    }
  }
}
