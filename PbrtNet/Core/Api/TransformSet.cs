// -----------------------------------------------------------------------
// <copyright file="TransformSet.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace PbrtNet.Core.Api
{
  public class TransformSet
  {
    public const int MaxTransforms = 2;

    private readonly List<Transform> _t = new List<Transform>(MaxTransforms);

    public Transform this[int i]
    {
      get
      {
        if (i < 0 || i >= MaxTransforms)
        {
          throw new ArgumentException();
        }

        return _t[i];
      }
      set
      {
        if (i < 0 || i >= MaxTransforms)
        {
          throw new ArgumentException();
        }

        _t[i] = value;
      }
    }

    public TransformSet Inverse()
    {
      TransformSet tInv = new TransformSet();
      for (int i = 0; i < MaxTransforms; ++i)
      {
        tInv._t[i] = _t[i].Inverse();
      }

      return tInv;
    }

    public bool IsAnimated()
    {
      for (int i = 0; i < MaxTransforms - 1; ++i)
      {
        if (_t[i] != _t[i + 1])
        {
          return true;
        }
      }

      return false;
    }
  }
}