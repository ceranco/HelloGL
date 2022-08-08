#nullable disable

using System.Numerics;

internal struct DirectionalLight
{
    public Vector3 Direction;
    public Vector3 Ambient;
    public Vector3 Diffuse;
    public Vector3 Specular;

    public DirectionalLight(Vector3 direction, Vector3 ambient, Vector3 diffuse, Vector3 specular)
    {
        Direction = direction;
        Ambient = ambient;
        Diffuse = diffuse;
        Specular = specular;
    }
}
