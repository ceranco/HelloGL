#nullable disable

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct Vertex
{
    public VertexPosition position;
    public VertexNormal normal;

    public Vertex(VertexPosition position, VertexNormal normal)
    {
        this.position = position;
        this.normal = normal;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct TextureCoord
{
    public float x;
    public float y;

    public TextureCoord(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
struct VertexColor
{
    public float r;
    public float g;
    public float b;

    public VertexColor(float r, float g, float b)
    {
        this.r = r;
        this.g = g;
        this.b = b;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VertexPosition
{
    public float x;
    public float y;
    public float z;

    public VertexPosition(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct VertexNormal
{
    public float x;
    public float y;
    public float z;

    public VertexNormal(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}
