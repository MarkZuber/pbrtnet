using System;
using System.Collections.Generic;
using System.Text;

namespace PbrtNet.Core.Api
{
  public class PbrtOptions
  {
    public string ImageFile { get; private set; }
    public bool QuickRender { get; private set; }
    public double[,] CropWindow { get; private set; }
  }
}
