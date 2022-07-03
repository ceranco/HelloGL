#nullable disable

using System.Runtime.InteropServices;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

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
    public VertexColor color;

    public Vertex(VertexPosition position, VertexColor color)
    {
        this.position = position;
        this.color = color;
    }
}

internal class Program
{
    private static readonly IWindow window = CreateWindow();
    private static GL Gl;

    private static readonly Vertex[] vertices =
    {
        new Vertex(new VertexPosition(0.5f, -0.5f, 0.0f), new VertexColor(1.0f, 0.0f, 0.0f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, 0.0f), new VertexColor(0.0f, 1.0f, 0.0f)),
        new Vertex(new VertexPosition(0.0f, 0.5f, 0.0f), new VertexColor(0.0f, 0.0f, 1.0f))
    };

    private static readonly uint[] indices = { 0, 1, 2 };

    private static VertexArrayObject<Vertex, uint> vao;
    private static BufferObject<Vertex> vbo;
    private static BufferObject<uint> ebo;
    private static ShaderProgram shader;
    private static bool polygonModeToggle = false;

    private static IWindow CreateWindow()
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "Hello OpenGL with Silk.NET";

        var window = Window.Create(options);
        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.Closing += OnClose;

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
            };
        }

        Gl = GL.GetApi(window);

        vbo = new BufferObject<Vertex>(Gl, vertices, BufferTargetARB.ArrayBuffer);
        ebo = new BufferObject<uint>(Gl, indices, BufferTargetARB.ElementArrayBuffer);

        vao = new VertexArrayObject<Vertex, uint>(Gl, vbo, ebo);
        vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float);
        vao.VertexAttributePointer(
            1,
            3,
            VertexAttribPointerType.Float,
            offset: sizeof(VertexPosition)
        );

        shader = ShaderProgram.FromFiles(Gl, "shader.vs", "shader.fs");
        shader.Set("offset", 0.0f);
    }

    private static void OnUpdate(double deltaTime)
    {
        shader.Set("offset", 0.5f * MathF.Sin((float)window.Time));
    }

    private static unsafe void OnRender(double deltaTime)
    {
        Gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        Gl.Clear(ClearBufferMask.ColorBufferBit);

        vao.Bind();
        shader.Use();
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
    }

    private static void Main()
    {
        window.Run();
    }
}
