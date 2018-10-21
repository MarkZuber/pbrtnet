using System;
using System.Collections.Generic;
using System.Text;

namespace PbrtNet.Core.Api
{
  public abstract class Integrator
  {
    public abstract void Render(Scene scene);
  }
}
