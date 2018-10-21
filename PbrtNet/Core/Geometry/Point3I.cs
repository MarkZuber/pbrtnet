using System;
using System.Collections.Generic;
using System.Text;

namespace PbrtNet.Core.Geometry
{
  public class Point3I
  {
    public Point3I()
    {
      X = 0;
      Y = 0;
      Z = 0;
    }

    public Point3I(int x, int y, int z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    public int X { get; }
    public int Y { get; }
    public int Z { get; }

    public override string ToString()
    {
      return string.Format($"[ {X}, {Y}, {Z} ]");
    }

    public int this[int i]
    {
      get
      {
        switch (i)
        {
        case 0:
          return X;
        case 1:
          return Y;
        case 2:
          return Z;
        default:
          throw new IndexOutOfRangeException();
        }
      }
    }

    public static Point3I operator -(Point3I a)
    {
      return new Point3I(-a.X, -a.Y, -a.Z);
    }
  }
}
