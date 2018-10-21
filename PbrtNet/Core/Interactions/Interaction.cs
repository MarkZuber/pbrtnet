using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Api;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Interactions
{
  public class Interaction
  {
    public Interaction()
    {
      P = new Point3D();
      Time = 0.0;
      PError = new Vector3D();
      Wo = new Vector3D();
      N = new Normal3D();
      MediumInterface = null;
    }

    public Interaction(
      Point3D p,
      Normal3D n,
      Vector3D pError,
      Vector3D wo,
      double time,
      MediumInterface mediumInterface)
    {
      P = p;
      N = n;
      PError = pError;
      Wo = wo;
      Time = time;
      MediumInterface = mediumInterface;
    }

    public Interaction(Point3D p, Vector3D wo, double time, MediumInterface mediumInterface)
    {
      P = p;
      Wo = wo;
      Time = time;
      MediumInterface = mediumInterface;
      PError = new Vector3D();
      N = new Normal3D();
    }

    public Interaction(Point3D p, double time, MediumInterface mediumInterface)
    {
      P = p;
      Time = time;
      MediumInterface = mediumInterface;
      Wo = new Vector3D();
      PError = new Vector3D();
      N = new Normal3D();
    }

    public Point3D P { get; }
    public double Time { get; }
    public Vector3D PError { get;}
    public Vector3D Wo { get; }
    public Normal3D N { get; set; }
    public MediumInterface MediumInterface { get; }

    // todo: possible problem area, need to validate comparison
    public bool IsSurfaceInteraction => N != new Normal3D();
    public bool IsMediumInteraction => !IsSurfaceInteraction;

    public Medium GetMedium(Vector3D w)
    {
      return w.Dot(N) > 0.0 ? MediumInterface.Outside : MediumInterface.Inside;
    }

    public Medium GetMedium()
    {
      // assert inside == outside
      return MediumInterface.Inside;
    }

    public Ray SpawnRay(Vector3D d)
    {
      Point3D o = P.OffsetRayOrigin(PError, N, d);
      return new Ray(o, d, double.MaxValue, Time, GetMedium(d));
    }

    public Ray SpawnRayTo(Point3D p2)
    {
      Point3D origin = P.OffsetRayOrigin(PError, N, (p2 - P).ToVector3D());
      Vector3D d = (p2 - P).ToVector3D();
      return new Ray(origin, d, 1.0 - PbrtMath.ShadowEpsilon, Time, GetMedium(d));
    }

    public Ray SpawnRayTo(Interaction it)
    {
      Point3D origin = P.OffsetRayOrigin(PError, N, (it.P - P).ToVector3D());
      Point3D target = it.P.OffsetRayOrigin(it.PError, it.N, (origin - it.P).ToVector3D());
      Vector3D d = (target - origin).ToVector3D();
      return new Ray(origin, d, 1.0 - PbrtMath.ShadowEpsilon, Time, GetMedium(d));
    }
  }
}
