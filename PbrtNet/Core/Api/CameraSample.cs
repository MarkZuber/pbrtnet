using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Api
{
  public class CameraSample
  {
    public CameraSample(Point2D filmPoint, Point2D lensPoint, double time)
    {
      FilmPoint = filmPoint;
      LensPoint = lensPoint;
      Time = time;
    }

    public Point2D FilmPoint { get; set; }
    public Point2D LensPoint { get; }
    public double Time { get; }
  }
}
