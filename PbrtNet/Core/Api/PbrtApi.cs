// -----------------------------------------------------------------------
// <copyright file="PbrtApi.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using PbrtNet.Core.Accelerators;
using PbrtNet.Core.Cameras;
using PbrtNet.Core.Filters;
using PbrtNet.Core.Samplers;

namespace PbrtNet.Core.Api
{
  public static class PbrtApi
  {
    public static Primitive MakeAccelerator(string name, IEnumerable<Primitive> prims, ParamSet paramSet)
    {
      Primitive accel = null;
      switch (name)
      {
      case "bvh":
        accel = BvhAccelerator.Create(prims, paramSet);
        break;
      //case "kdtree":
      //  accel = CreateKdTreeAccelerator(prims, paramSet);
      //  break;
      default:
        //Warning("Accelerator \"%s\" unknown.", name.c_str());
        break;
      }

      paramSet.ReportUnused();
      return accel;
    }

    public static Camera MakeCamera(
      string name,
      ParamSet paramSet,
      TransformSet cam2worldSet,
      double transformStart,
      double transformEnd,
      Film film)
    {
      Camera camera = null;
      MediumInterface mediumInterface = new MediumInterface(); // todo: implement graphicsState graphicsState.CreateMediumInterface();
      //static_assert(MaxTransforms == 2,
      //              "TransformCache assumes only two transforms");
      Transform[] cam2world = new Transform[]
      {
        // todo: implement transform cache
        //transformCache.Lookup(cam2worldSet[0]),
        //transformCache.Lookup(cam2worldSet[1])
        cam2worldSet[0],
        cam2worldSet[1]
      };

      AnimatedTransform animatedCam2World = new AnimatedTransform(
        cam2world[0],
        transformStart,
        cam2world[1],
        transformEnd);
      switch (name)
      {
      case "perspective":
        camera = PerspectiveCamera.Create(paramSet, animatedCam2World, film, mediumInterface.Outside);
        break;
      //case "orthographic":
      //  camera = CreateOrthographicCamera(paramSet, animatedCam2World, film, mediumInterface.Outside);
      //  break;
      //case "realistic":
      //  camera = CreateRealisticCamera(paramSet, animatedCam2World, film, mediumInterface.Outside);
      //  break;
      //case "environment":
      //  camera = CreateEnvironmentCamera(paramSet, animatedCam2World, film, mediumInterface.Outside);
      //  break;
      default:
        //Warning("Camera \"%s\" unknown.", name.c_str());
        break;
      }

      paramSet.ReportUnused();
      return camera;
    }

    public static Sampler MakeSampler(string name, ParamSet paramSet, Film film)
    {
      Sampler sampler = null;
      switch (name)
      {
      //case "lowdiscrepancy":
      //case "02sequence":
      //  sampler = CreateZeroTwoSequenceSampler(paramSet);
      //  break;
      //case "maxmindist":
      //  sampler = CreateMaxMinDistSampler(paramSet);
      //  break;
      //case "halton":
      //  sampler = CreateHaltonSampler(paramSet, film.GetSampleBounds());
      //  break;
      //case "sobol":
      //  sampler = CreateSobolSampler(paramSet, film.GetSampleBounds());
      //  break;
      case "random":
        sampler = RandomSampler.Create(paramSet);
        break;
      //case "stratified":
      //  sampler = CreateStratifiedSampler(paramSet);
      //  break;
      default:
        //Warning("Sampler \"%s\" unknown.", name.c_str());
        break;
      }

      paramSet.ReportUnused();
      return sampler;
    }

    public static Filter MakeFilter(string name, ParamSet paramSet)
    {
      Filter filter = null;
      switch (name)
      {
      case "box":
        filter = BoxFilter.Create(paramSet);
        break;
      //case "gaussian":
      //  filter = CreateGaussianFilter(paramSet);
      //  break;
      //case "mitchell":
      //  filter = CreateMitchellFilter(paramSet);
      //  break;
      //case "sinc":
      //  filter = CreateSincFilter(paramSet);
      //  break;
      //case "triangle":
      //  filter = CreateTriangleFilter(paramSet);
      //  break;
      default:
        //Error("Filter \"%s\" unknown.", name.c_str());
        //exit(1);
        break;
      }

      paramSet.ReportUnused();
      return filter;
    }

    public static Film MakeFilm(PbrtOptions options, string name, ParamSet paramSet, Filter filter)
    {
      Film film = null;
      switch (name)
      {
      case "image":
        film = Film.Create(options, paramSet, filter);
        break;
      default:
        //Warning("Film \"%s\" unknown.", name.c_str());
        break;
      }

      paramSet.ReportUnused();
      return film;
    }
  }
}