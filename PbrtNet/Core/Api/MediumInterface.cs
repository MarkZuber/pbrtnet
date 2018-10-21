using System;
using System.Collections.Generic;
using System.Text;

namespace PbrtNet.Core.Api
{
    public class MediumInterface
    {
      public MediumInterface()
      {
        Inside = null;
        Outside = null;
      }

      public MediumInterface(Medium medium)
      {
        Inside = medium;
        Outside = medium;
      }

      public MediumInterface(Medium inside, Medium outside)
      {
        Inside = inside;
        Outside = outside;
      }

      public bool IsMediumTransition => Inside != Outside;

      public Medium Inside { get; }
      public Medium Outside { get; }
    }
}
