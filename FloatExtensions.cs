internal static class FloatExtensions
{
    public static float ToRadians(this float degrees) => degrees * MathF.PI / 180.0f;

    public static float Clamp(this float val, float min, float max) =>
        MathF.Max(MathF.Min(val, max), min);
}
