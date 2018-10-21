// -----------------------------------------------------------------------
// <copyright file="ProjectiveCamera.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using PbrtNet.Core.Api;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Cameras
{
  public abstract class ProjectiveCamera : Camera
  {
    // ProjectiveCamera Public Methods
    protected ProjectiveCamera(
      AnimatedTransform cameraToWorld,
      Transform cameraToScreen,
      Bounds2D screenWindow,
      double shutterOpen,
      double shutterClose,
      double lensr,
      double focald,
      Film film,
      Medium medium)
      : base(cameraToWorld, shutterOpen, shutterClose, film, medium)
    {
      CameraToScreen = cameraToScreen;
      // Initialize depth of field parameters
      LensRadius = lensr;
      FocalDistance = focald;

      // Compute projective camera transformations

      // Compute projective camera screen transformations
      ScreenToRaster = Transform.Scale(film.FullResolution.X, film.FullResolution.Y, 1) * Transform.Scale(
                         1 / (screenWindow.MaxPoint.X - screenWindow.MinPoint.X),
                         1 / (screenWindow.MinPoint.Y - screenWindow.MaxPoint.Y),
                         1) * Transform.Translate(new Vector3D(-screenWindow.MinPoint.X, -screenWindow.MaxPoint.Y, 0));
      RasterToScreen = ScreenToRaster.Inverse();
      RasterToCamera = CameraToScreen.Inverse() * RasterToScreen;
    }

    // ProjectiveCamera Protected Data
    public Transform CameraToScreen { get; }
    public Transform RasterToCamera { get; }
    public Transform ScreenToRaster { get; }
    public Transform RasterToScreen { get; }
    public double LensRadius { get; }
    public double FocalDistance { get; }
  };
}