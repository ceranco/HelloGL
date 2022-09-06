#nullable disable

using System.Numerics;
using Silk.NET.Maths;

internal static class VectorExtensions
{
    public static Vector3D<float> ToVectorD(this Vector3 vec) => new(vec.X, vec.Y, vec.Z);

    public static Vector3 ToVector(this Vector3D<float> vec) => new(vec.X, vec.Y, vec.Z);

    public static Vector2D<float> ToVectorD(this Vector2 vec) => new(vec.X, vec.Y);

    public static Vector2 ToVector(this Vector2D<float> vec) => new(vec.X, vec.Y);

    public static Vector2 ToVector(this Vector2D<int> vec) => new(vec.X, vec.Y);
}
