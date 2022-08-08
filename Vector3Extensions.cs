#nullable disable

using System.Numerics;
using Silk.NET.Maths;

internal static class Vector3Extensions
{
    public static Vector3D<float> ToVector3D(this Vector3 vec) => new(vec.X, vec.Y, vec.Z);
}
