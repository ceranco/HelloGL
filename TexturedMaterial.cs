#nullable disable

using System.Numerics;
using Silk.NET.OpenGL;

internal struct TexturedMaterial
{
    public TextureUnit Diffuse;
    public TextureUnit Specular;
    public TextureUnit Emission;
    public float Shininess;

    public TexturedMaterial(
        TextureUnit diffuse,
        TextureUnit specular,
        TextureUnit emission,
        float shininess
    )
    {
        Diffuse = diffuse;
        Specular = specular;
        Emission = emission;
        Shininess = shininess;
    }
}
