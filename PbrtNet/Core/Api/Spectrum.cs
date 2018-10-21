using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PbrtNet.Core.Api
{
  // RGB Spectrum
  // todo: add #ifdef to allow for SampledSpectrum as well...
  public class Spectrum
  {
    private readonly double _r;
    private readonly double _g;
    private readonly double _b;

    public static Spectrum Create(double val)
    {
      return new Spectrum(val);
    }

    public static Spectrum FromRgb(double r, double g, double b)
    {
      return new Spectrum(r, g, b);
    }

    public static Spectrum FromXyz(double x, double y, double z)
    {
      double[] rgb = XyzToRgb(x, y, z);
      return new Spectrum(rgb[0], rgb[1], rgb[2]);
    }

    private Spectrum(double r, double g, double b)
    {
      _r = r;
      _g = g;
      _b = b;
    }

    public Spectrum(double val)
    {
      _r = val;
      _g = val;
      _b = val;
    }

    public (double r, double g, double b) ToRgb()
    {
      return (_r, _g, _b);
    }

    private static double[] RgbToXyz(double r, double g, double b)
    {
      double x = 0.412453f * r + 0.357580f * g + 0.180423f * b;
      double y = 0.212671f * r + 0.715160f * g + 0.072169f * b;
      double z = 0.019334f * r + 0.119193f * g + 0.950227f * b;
      return new double[3] {x, y, z};
    }

    public static double[] XyzToRgb(double[] xyz)
    {
      return XyzToRgb(xyz[0], xyz[1], xyz[2]);
    }

    private static double[] XyzToRgb(double x, double y, double z)
    {
      double r = 3.240479f * x - 1.537150f * y - 0.498535f * z;
      double g = -0.969256f * x + 1.875991f * y + 0.041556f * z;
      double b = 0.055648f * x - 0.204043f * y + 1.057311f * z;
      return new double[3] {r, g, b};
    }


  public double[] ToXyz()
    {
      return RgbToXyz(_r, _g, _b);
    }

    public double Y()
    {
      // const double YWeight[3] = { 0.212671f, 0.715160f, 0.072169f };
      return 0.212671 * _r + 0.715160 * _g + 0.072169 * _b;
    }

    public bool IsBlack()
    {
      return (_r == 0.0 && _g == 0.0 && _b == 0.0);
    }

    public static Spectrum operator *(Spectrum s1, Spectrum s2)
    {
      return new Spectrum(s1._r * s2._r, s1._g * s2._g, s1._b * s2._b);
    }

    public static Spectrum operator *(Spectrum s, double scalar)
    {
      return new Spectrum(s._r * scalar, s._g * scalar, s._b * scalar);
    }

    public static Spectrum operator *(double scalar, Spectrum s)
    {
      return new Spectrum(s._r * scalar, s._g * scalar, s._b * scalar);
    }

    public static Spectrum operator /(Spectrum s, double scalar)
    {
      double inv = 1.0 / scalar;
      return new Spectrum(s._r * inv, s._g * inv, s._b * inv);
    }

    public static Spectrum operator +(Spectrum s1, Spectrum s2)
    {
      return new Spectrum(s1._r + s2._r, s1._g + s2._g, s1._b + s2._b);
    }

    public static Spectrum operator -(Spectrum s1, Spectrum s2)
    {
      return new Spectrum(s1._r - s2._r, s1._g - s2._g, s1._b - s2._b);
    }

    public static Spectrum operator -(Spectrum s)
    {
      return new Spectrum(-s._r, -s._g, -s._b);
    }

    public bool HasNaNs()
    {
      return double.IsNaN(_r) || double.IsNaN(_g) || double.IsNaN(_b);
    }
  }
}
