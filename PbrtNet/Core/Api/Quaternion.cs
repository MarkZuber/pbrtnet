using System;
using System.Collections.Generic;
using System.Text;
using PbrtNet.Core.Geometry;

namespace PbrtNet.Core.Api
{
  public class Quaternion
  {
    private readonly Vector3D _v;
    private readonly double _w;

    public Quaternion()
    {
      _v = new Vector3D();
      _w = 1.0;
    }

    private Quaternion(Vector3D v, double w)
    {
      _v = v;
      _w = w;
    }

    public Transform ToTransform()
    {
      double xx = _v.X * _v.X, yy = _v.Y * _v.Y, zz = _v.Z * _v.Z;
      double xy = _v.X * _v.Y, xz = _v.X * _v.Z, yz = _v.Y * _v.Z;
      double wx = _v.X * _w, wy = _v.Y * _w, wz = _v.Z * _w;

      Matrix4x4 m = new Matrix4x4(
        1.0 - 2.0 * (yy + zz), 2.0 * (xy + wz), 2.0 * (xz - wy), 0.0,
        2.0 * (xy - wz), 1.0 - 2.0 * (xx + zz), 2.0 * (yz + wx), 0.0,
        2.0 * (xz + wy), 2.0 * (yz - wx), 1.0 - 2.0 * (xx + yy), 0.0,
        0.0, 0.0, 0.0, 0.0);

      // Transpose since we are left-handed.  Ugh.
      return new Transform(m.Transpose(), m);

    }

    public static Quaternion operator +(Quaternion q1, Quaternion q2)
    {
      return new Quaternion(q1._v + q2._v, q1._w + q2._w);
    }

    public static Quaternion operator -(Quaternion q1, Quaternion q2)
    {
      return new Quaternion(q1._v - q2._v, q1._w - q2._w);
    }

    public static Quaternion operator -(Quaternion q)
    {
      return new Quaternion(-q._v, -q._w);
    }

    public static Quaternion operator *(Quaternion q, double scalar)
    {
      return new Quaternion(q._v * scalar, q._w * scalar);
    }
    public static Quaternion operator *(double scalar, Quaternion q)
    {
      return new Quaternion(q._v * scalar, q._w * scalar);
    }
    public static Quaternion operator /(Quaternion q, double scalar)
    {
      return new Quaternion(q._v / scalar, q._w / scalar);
    }

    public double Dot(Quaternion q)
    {
      return _v.Dot(q._v) + _w * q._w;
    }

    public Quaternion Normalize()
    {
      return this / Math.Sqrt(this.Dot(this));
    }

    /// <inheritdoc />
    public override string ToString()
    {
      return $"[ {_v.X}, {_v.Y}, {_v.Z}, {_w} ]";
    }
  }
}
