#nullable disable

using System.Numerics;

internal struct PointLight
{
    public Vector3 Position;
    public Vector3 Ambient;
    public Vector3 Diffuse;
    public Vector3 Specular;

    public float Constant;
    public float Linear;
    public float Quadratic;

    public PointLight(
        Vector3 position,
        Vector3 ambient,
        Vector3 diffuse,
        Vector3 specular,
        float constant,
        float linear,
        float quadratic
    )
    {
        Position = position;
        Ambient = ambient;
        Diffuse = diffuse;
        Specular = specular;
        Constant = constant;
        Linear = linear;
        Quadratic = quadratic;
    }
}
