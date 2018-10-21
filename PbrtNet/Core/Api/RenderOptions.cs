using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PbrtNet.Core.Accelerators;
using PbrtNet.Core.Integrators;

namespace PbrtNet.Core.Api
{
  public class RenderOptions
  {
    public RenderOptions()
    {
      FilterName = "box";
      FilmName = "image";
      SamplerName = "halton";
      AcceleratorName = "bvh";
      IntegratorName = "path";
    }

    public TransformSet CameraToWorld { get; set; }
    public List<Light> Lights { get; } = new List<Light>();
    public List<Primitive> Primitives { get; } = new List<Primitive>();
    public bool HasScatteringMedia { get; set; }
    public double TransformStartTime { get; set; }= 0.0;
    public double TransformEndTime { get; set; }= 1.0;

    public string FilterName { get; set; }
    public ParamSet FilterParams { get; } = new ParamSet();

    public string FilmName { get; set; }
    public ParamSet FilmParams { get; } = new ParamSet();

    public string SamplerName { get; set; }
    public ParamSet SamplerParams { get; } = new ParamSet();

    public string AcceleratorName { get; set; }
    public ParamSet AcceleratorParams { get; } = new ParamSet();

    public string IntegratorName { get; set; }
    public ParamSet IntegratorParams { get; } = new ParamSet();

    public string CameraName { get; set; }
    public ParamSet CameraParams { get; } = new ParamSet();

    public Integrator MakeIntegrator(PbrtOptions options)
    {
      Camera camera = MakeCamera(options);
      if (camera == null)
      {
        //Error("Unable to create camera");
        return null;
      }

      Sampler sampler =
          PbrtApi.MakeSampler(SamplerName, SamplerParams, camera.Film);
      if (sampler == null)
      {
        //Error("Unable to create sampler.");
        return null;
      }

      Integrator integrator = null;
      switch (IntegratorName)
      {
      case "whitted":
        integrator = WhittedIntegrator.Create(IntegratorParams, sampler, camera);
        break;
      //case "directlighting":
      //  integrator =
      //    CreateDirectLightingIntegrator(IntegratorParams, sampler, camera);
      //  break;
      //case "path":
      //  integrator = CreatePathIntegrator(IntegratorParams, sampler, camera);
      //  break;
      //case "volpath":
      //  integrator = CreateVolPathIntegrator(IntegratorParams, sampler, camera);
      //  break;
      //case "bdpt":
      //  integrator = CreateBDPTIntegrator(IntegratorParams, sampler, camera);
      //  break;
      //case "mlt":
      //  integrator = CreateMLTIntegrator(IntegratorParams, camera);
      //  break;
      //case "ambientocclusion":
      //  integrator = CreateAOIntegrator(IntegratorParams, sampler, camera);
      //  break;
      //case "sppm":
      //  integrator = CreateSPPMIntegrator(IntegratorParams, camera);
      //  break;
      default:
        //Error("Integrator \"%s\" unknown.", IntegratorName.c_str());
        return null;
      }

      if (HasScatteringMedia && IntegratorName != "volpath" &&
          IntegratorName != "bdpt" && IntegratorName != "mlt")
      {
        //Warning(
        //    "Scene has scattering media but \"%s\" integrator doesn't support "
        //    "volume scattering. Consider using \"volpath\", \"bdpt\", or "
        //    "\"mlt\".", IntegratorName.c_str());
      }

      IntegratorParams.ReportUnused();
      // Warn if no light sources are defined
      if (!Lights.Any())
      {
        //Warning(
        //  "No light sources defined in scene; "
        //"rendering a black image.");
      }

      return integrator;

    }

    public Scene MakeScene()
    {
      Primitive accelerator =
        PbrtApi.MakeAccelerator(AcceleratorName, Primitives, AcceleratorParams);
      if (accelerator == null)
      {
        accelerator = new BvhAccelerator(Primitives);
      }
      Scene scene = new Scene(accelerator, Lights);
      // Erase primitives and lights from _RenderOptions_
      Primitives.Clear();
      Lights.Clear();
      return scene;
    }

    public Camera MakeCamera(PbrtOptions options)
    {
      Filter filter = PbrtApi.MakeFilter(FilterName, FilterParams);
      Film film = PbrtApi.MakeFilm(options, FilmName, FilmParams, filter);
      if (film == null)
      {
        //Error("Unable to create film.");
        return null;
      }
      Camera camera = PbrtApi.MakeCamera(CameraName, CameraParams, CameraToWorld,
                                        TransformStartTime,
                                        TransformEndTime, film);
      return camera;
    }
  }
}
