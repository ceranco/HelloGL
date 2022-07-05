#nullable disable

using System.Runtime.InteropServices;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using StbImageSharp;

[StructLayout(LayoutKind.Sequential, Pack = 0)]
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

[StructLayout(LayoutKind.Sequential, Pack = 0)]
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

[StructLayout(LayoutKind.Sequential, Pack = 0)]
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

[StructLayout(LayoutKind.Sequential, Pack = 0)]
struct Vertex
{
    public VertexPosition position;
    public TextureCoord textureCoord;

    public Vertex(VertexPosition position, VertexColor color, TextureCoord textureCoord)
    {
        this.position = position;
        this.textureCoord = textureCoord;
    }
}

internal class Program
{
    private static readonly IWindow window = CreateWindow();
    private static GL Gl;

    private static readonly Vertex[] vertices =
    {
        new Vertex(
            new VertexPosition(0.5f, 0.5f, 0.0f),
            new VertexColor(1.0f, 0.0f, 0.0f),
            new TextureCoord(1.0f, 1.0f)
        ), // top right
        new Vertex(
            new VertexPosition(0.5f, -0.5f, 0.0f),
            new VertexColor(0.0f, 1.0f, 0.0f),
            new TextureCoord(1.0f, 0.0f)
        ), // bottom right
        new Vertex(
            new VertexPosition(-0.5f, -0.5f, 0.0f),
            new VertexColor(0.0f, 0.0f, 1.0f),
            new TextureCoord(0.0f, 0.0f)
        ), // bottom left
        new Vertex(
            new VertexPosition(-0.5f, 0.5f, 0.0f),
            new VertexColor(1.0f, 1.0f, 0.0f),
            new TextureCoord(0.0f, 1.0f)
        ) // top left
    };

    private static readonly uint[] indices = { 0, 1, 2, 0, 2, 3 };

    private static VertexArrayObject<Vertex, uint> vao;
    private static BufferObject<Vertex> vbo;
    private static BufferObject<uint> ebo;
    private static ShaderProgram shader;
    private static Texture texture;
    private static bool polygonModeToggle = false;
    private static float mixValue = 0.0f;
    private static readonly Transform transform1 = new();
    private static readonly Transform transform2 = new();

    private static IWindow CreateWindow()
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1600, 1600);
        options.Title = "Hello OpenGL with Silk.NET";

        var window = Window.Create(options);
        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.Closing += OnClose;
        window.Resize += OnResize;

        return window;
    }

    private static unsafe void OnLoad()
    {
        var input = window.CreateInput();
        foreach (var keyboard in input.Keyboards)
        {
            keyboard.KeyDown += (keyboard, key, _) =>
            {
                if (key == Key.Escape)
                {
                    window.Close();
                }
                else if (key == Key.Space || key == Key.Enter)
                {
                    Gl.PolygonMode(
                        MaterialFace.FrontAndBack,
                        polygonModeToggle ? PolygonMode.Fill : PolygonMode.Line
                    );
                    polygonModeToggle = !polygonModeToggle;
                }
                else if (key == Key.Left)
                {
                    mixValue -= 0.1f;
                    mixValue = mixValue < 0.0f ? 0.0f : mixValue;
                    shader.Set("mixValue", mixValue);
                }
                else if (key == Key.Right)
                {
                    mixValue += 0.1f;
                    mixValue = mixValue > 1.0f ? 1.0f : mixValue;
                    shader.Set("mixValue", mixValue);
                }
            };
        }

        Gl = GL.GetApi(window);

        vbo = new BufferObject<Vertex>(Gl, vertices, BufferTargetARB.ArrayBuffer);
        ebo = new BufferObject<uint>(Gl, indices, BufferTargetARB.ElementArrayBuffer);

        vao = new VertexArrayObject<Vertex, uint>(Gl, vbo, ebo);
        vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float);
        vao.VertexAttributePointer(
            1,
            2,
            VertexAttribPointerType.Float,
            offset: sizeof(VertexPosition)
        );

        shader = ShaderProgram.FromFiles(Gl, "shader.vs", "shader.fs");
        shader.Set("transform", Matrix4X4<float>.Identity);

        texture = Texture.FromFile(Gl, "wall.jpg");
        shader.Set("texture1", 0);
    }

    private static void OnUpdate(double deltaTime)
    {
        transform1.Rotation = Quaternion<float>.CreateFromAxisAngle(
            Vector3D<float>.UnitZ,
            (float)(window.Time)
        );
        transform1.Translation = new Vector3D<float>(0.5f, -0.5f, 0.0f);

        transform2.Scale = 0.5f * MathF.Sin((float)window.Time) + 0.5f;
        transform2.Translation = new Vector3D<float>(-0.5f, 0.5f, 0.0f);
    }

    private static unsafe void OnRender(double deltaTime)
    {
        Gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        Gl.Clear(ClearBufferMask.ColorBufferBit);

        vao.Bind();
        shader.Use();

        texture.Use(TextureUnit.Texture0);

        shader.Set("transform", transform1.Matrix);
        Gl.DrawElements(
            PrimitiveType.Triangles,
            (uint)indices.Length,
            DrawElementsType.UnsignedInt,
            null
        );

        shader.Set("transform", transform2.Matrix);
        Gl.DrawElements(
            PrimitiveType.Triangles,
            (uint)indices.Length,
            DrawElementsType.UnsignedInt,
            null
        );
    }

    private static void OnClose()
    {
        vao.Dispose();
        vbo.Dispose();
        ebo.Dispose();
        shader.Dispose();
        texture.Dispose();
    }

    private static void OnResize(Vector2D<int> size) => Gl.Viewport(size);

    private static void Main()
    {
        window.Run();
    }
}
