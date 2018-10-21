// -----------------------------------------------------------------------
// <copyright file="Distribution1D.cs" company="ZubeNET">
//   Copyright...
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace PbrtNet.Core
{
  public class Distribution1D
  {
    private readonly List<double> _cdf;
    private readonly List<double> _func;
    private readonly double _funcInt;

    public Distribution1D(IEnumerable<double> f)
    {
      _func = f.ToList();
      _cdf = new List<double>(_func.Count + 1);

      _cdf[0] = 0.0;
      for (int i = 1; i < _func.Count + 1; ++i)
      {
        _cdf[i] = _cdf[i - 1] + _func[i - 1] / _func.Count;
      }

      // Transform step function integral into CDF
      _funcInt = _cdf[_func.Count];
      if (_funcInt == 0.0)
      {
        for (int i = 1; i < _func.Count + 1; ++i)
        {
          _cdf[i] = Convert.ToDouble(i) / Convert.ToDouble(_func.Count);
        }
      }
      else
      {
        for (int i = 1; i < _func.Count + 1; ++i)
        {
          _cdf[i] /= _funcInt;
        }
      }
    }

    public int Count => _func.Count;
  }
}