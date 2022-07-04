using Silk.NET.OpenGL;
using StbImageSharp;

internal class Texture : IDisposable
{
    private readonly GL Gl;
    private readonly uint texture;

    static Texture()
    {
        StbImage.stbi_set_flip_vertically_on_load(1);
    }

    public static Texture FromFile(GL Gl, string path)
    {
        using var file = File.Open(path, FileMode.Open);
        var image = ImageResult.FromStream(file);
        var format = image.Comp.ToGLEnum();

        return new Texture(Gl, image.Data, (uint)image.Width, (uint)image.Height, format);
    }

    public Texture(GL Gl, Span<byte> data, uint width, uint height, GLEnum format)
    {
        this.Gl = Gl;

        texture = Gl.GenTexture();
        Gl.BindTexture(TextureTarget.Texture2D, texture);
        Gl.TexImage2D<byte>(
            TextureTarget.Texture2D,
            0,
            (int)format,
            width,
            height,
            0,
            format,
            PixelType.UnsignedByte,
            data
        );

        Gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS,
            (int)GLEnum.ClampToEdge
        );
        Gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT,
            (int)GLEnum.ClampToEdge
        );
        Gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)GLEnum.Linear
        );
        Gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)GLEnum.Linear
        );
        Gl.GenerateMipmap(TextureTarget.Texture2D);
    }

    public void Use(TextureUnit unit)
    {
        Gl.ActiveTexture(unit);
        Gl.BindTexture(TextureTarget.Texture2D, texture);
    }

    public void Dispose() => Gl.DeleteTexture(texture);
}

internal static class ColorComponentsExtensions
{
    public static GLEnum ToGLEnum(this ColorComponents components) =>
        components switch
        {
            ColorComponents.RedGreenBlue => GLEnum.Rgb,
            ColorComponents.RedGreenBlueAlpha => GLEnum.Rgba,
            _ => throw new NotImplementedException(),
        };
}
