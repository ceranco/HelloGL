#nullable disable

using System.Numerics;
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

    public Vertex(VertexPosition position)
    {
        this.position = position;
    }
}

internal class Program
{
    private static readonly IWindow window = CreateWindow();
    private static readonly KeyState keyState = new();
    private static readonly MouseState mouseState = new();
    private static GL Gl;

    private static readonly Vertex[] vertices =
    {
        new Vertex(new VertexPosition(-0.5f, -0.5f, -0.5f)),
        new Vertex(new VertexPosition(0.5f, -0.5f, -0.5f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, -0.5f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, -0.5f)),
        new Vertex(new VertexPosition(-0.5f, 0.5f, -0.5f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, -0.5f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, 0.5f)),
        new Vertex(new VertexPosition(0.5f, -0.5f, 0.5f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, 0.5f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, 0.5f)),
        new Vertex(new VertexPosition(-0.5f, 0.5f, 0.5f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, 0.5f)),
        new Vertex(new VertexPosition(-0.5f, 0.5f, 0.5f)),
        new Vertex(new VertexPosition(-0.5f, 0.5f, -0.5f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, -0.5f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, -0.5f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, 0.5f)),
        new Vertex(new VertexPosition(-0.5f, 0.5f, 0.5f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, 0.5f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, -0.5f)),
        new Vertex(new VertexPosition(0.5f, -0.5f, -0.5f)),
        new Vertex(new VertexPosition(0.5f, -0.5f, -0.5f)),
        new Vertex(new VertexPosition(0.5f, -0.5f, 0.5f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, 0.5f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, -0.5f)),
        new Vertex(new VertexPosition(0.5f, -0.5f, -0.5f)),
        new Vertex(new VertexPosition(0.5f, -0.5f, 0.5f)),
        new Vertex(new VertexPosition(0.5f, -0.5f, 0.5f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, 0.5f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, -0.5f)),
        new Vertex(new VertexPosition(-0.5f, 0.5f, -0.5f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, -0.5f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, 0.5f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, 0.5f)),
        new Vertex(new VertexPosition(-0.5f, 0.5f, 0.5f)),
        new Vertex(new VertexPosition(-0.5f, 0.5f, -0.5f))
    };

    private static readonly Camera camera = new(new(1600, 1600));

    private static VertexArrayObject<Vertex, uint> modelVao;
    private static VertexArrayObject<Vertex, uint> lightVao;
    private static BufferObject<Vertex> boxVbo;
    private static ShaderProgram modelShader;
    private static ShaderProgram lightShader;
    private static bool polygonModeToggle = false;

    private static readonly Transform lightTransform =
        new() { Translation = new Vector3D<float>(1.2f, 1.0f, 2.0f), Scale = 0.2f };

    private static IWindow CreateWindow()
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1600, 1600);
        options.Title = "Hello OpenGL with Silk.NET";
        options.PreferredDepthBufferBits = 24;

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
            keyboard.KeyDown += (_, key, _) =>
            {
                keyState.KeyDown(key);
                switch (key)
                {
                    case Key.Escape:
                        window.Close();
                        break;
                    case Key.Space:
                    case Key.Enter:
                        Gl.PolygonMode(
                            MaterialFace.FrontAndBack,
                            polygonModeToggle ? PolygonMode.Fill : PolygonMode.Line
                        );
                        polygonModeToggle = !polygonModeToggle;
                        break;
                }
            };
            keyboard.KeyUp += (_, key, _) => keyState.KeyUp(key);
        }
        foreach (var mouse in input.Mice)
        {
            mouse.Cursor.CursorMode = CursorMode.Raw;
            mouse.MouseMove += (_, position) => mouseState.MouseMove(position);
            mouse.Scroll += (_, delta) =>
                mouseState.MouseScroll(delta.Y > 0 ? ScrollDirection.Up : ScrollDirection.Down);
        }

        Gl = GL.GetApi(window);

        boxVbo = new BufferObject<Vertex>(Gl, vertices, BufferTargetARB.ArrayBuffer);

        modelVao = new VertexArrayObject<Vertex, uint>(Gl, boxVbo);
        modelVao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float);

        lightVao = new VertexArrayObject<Vertex, uint>(Gl, boxVbo);
        lightVao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float);

        modelShader = ShaderProgram.FromFiles(Gl, "shader.vs", "model.fs");
        modelShader.Set("objectColor", 1.0f, 0.5f, 0.31f);
        modelShader.Set("lightColor", 1.0f, 1.0f, 1.0f);

        lightShader = ShaderProgram.FromFiles(Gl, "shader.vs", "light.fs");

        Gl.Enable(EnableCap.DepthTest);
    }

    private static void OnUpdate(double deltaTime)
    {
        camera.Update(deltaTime, keyState, mouseState);

        modelShader.Set("model", Matrix4X4<float>.Identity);
        modelShader.Set("view", camera.ViewMatrix);
        modelShader.Set("projection", camera.ProjectionMatrix);

        lightShader.Set("model", lightTransform.Matrix);
        lightShader.Set("view", camera.ViewMatrix);
        lightShader.Set("projection", camera.ProjectionMatrix);
    }

    private static unsafe void OnRender(double deltaTime)
    {
        Gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        modelVao.Bind();
        modelShader.Use();
        Gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)vertices.Length);

        lightVao.Bind();
        lightShader.Use();
        Gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)vertices.Length);
    }

    private static void OnClose()
    {
        modelVao.Dispose();
        lightVao.Dispose();
        boxVbo.Dispose();
        modelShader.Dispose();
        lightShader.Dispose();
    }

    private static void OnResize(Vector2D<int> size)
    {
        Gl.Viewport(size);
        camera.WindowSize = size;
    }

    private static void Main()
    {
        window.Run();
    }
}
