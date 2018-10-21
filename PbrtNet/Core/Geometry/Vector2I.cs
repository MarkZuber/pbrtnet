using System;

namespace PbrtNet.Core.Geometry
{
  public class Vector2I
  {
    public Vector2I()
    {
      X = 0;
      Y = 0;
    }

    public Vector2I(int x, int y)
    {
      X = x;
      Y = y;
    }

    public Vector2I(Vector2I other)
    {
      X = other.X;
      Y = other.Y;
    }

    public int X { get; set; }
    public int Y { get; set; }

    public bool HasNaNs => false;

    /// <inheritdoc />
    public override string ToString()
    {
      return $"[ {X}, {Y} ]";
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
          default:
            throw new IndexOutOfRangeException();
        }
      }
      set
      {
        switch (i)
        {
          case 0:
            X = value;
            break;
          case 1:
            Y = value;
            break;
          default:
            throw new IndexOutOfRangeException();
        }
      }
    }

    public static Vector2I operator +(Vector2I a, Vector2I b)
    {
      return new Vector2I(a.X + b.X, a.Y + b.Y);
    }

    public static Vector2I operator -(Vector2I a, Vector2I b)
    {
      return new Vector2I(a.X - b.X, a.Y - b.Y);
    }

    public static Vector2I operator -(Vector2I a)
    {
      return new Vector2I(-a.X, -a.Y);
    }

    public static Vector2I operator *(Vector2I a, int b)
    {
      return new Vector2I(a.X * b, a.Y * b);
    }

    public static Vector2I operator *(int b, Vector2I a)
    {
      return new Vector2I(a.X * b, a.Y * b);
    }

    public static Vector2I operator /(Vector2I a, int b)
    {
      int inv = 1 / b;
      return new Vector2I(a.X * inv, a.Y * inv);
    }

    public int LengthSquared => X * X + Y * Y;
    public int Length => Convert.ToInt32(Math.Sqrt(LengthSquared));

    public int Dot(Vector2I other)
    {
      return X * other.X + Y * other.Y;
    }

    public int AbsDot(Vector2I other)
    {
      return Math.Abs(Dot(other));
    }

    public Vector2I Normalize()
    {
      return this / Length;
    }

    public Vector2I Abs()
    {
      return new Vector2I(Math.Abs(X), Math.Abs(Y));
    }
  }
}
