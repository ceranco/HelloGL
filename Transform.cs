using Silk.NET.Maths;

internal class Transform
{
    public Vector3D<float> Translation { get; set; } = new Vector3D<float>(1.0f);
    public float Scale { get; set; } = 1.0f;
    public Quaternion<float> Rotation { get; set; } = Quaternion<float>.Identity;

    public Matrix4X4<float> Matrix =>
        Matrix4X4.CreateFromQuaternion(Rotation)
        * Matrix4X4.CreateScale(Scale)
        * Matrix4X4.CreateTranslation(Translation);
}
