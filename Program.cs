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

    public Vertex(VertexPosition position, TextureCoord textureCoord)
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
        new Vertex(new VertexPosition(-0.5f, -0.5f, -0.5f), new TextureCoord(0.0f, 0.0f)),
        new Vertex(new VertexPosition(0.5f, -0.5f, -0.5f), new TextureCoord(1.0f, 0.0f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, -0.5f), new TextureCoord(1.0f, 1.0f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, -0.5f), new TextureCoord(1.0f, 1.0f)),
        new Vertex(new VertexPosition(-0.5f, 0.5f, -0.5f), new TextureCoord(0.0f, 1.0f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, -0.5f), new TextureCoord(0.0f, 0.0f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, 0.5f), new TextureCoord(0.0f, 0.0f)),
        new Vertex(new VertexPosition(0.5f, -0.5f, 0.5f), new TextureCoord(1.0f, 0.0f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, 0.5f), new TextureCoord(1.0f, 1.0f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, 0.5f), new TextureCoord(1.0f, 1.0f)),
        new Vertex(new VertexPosition(-0.5f, 0.5f, 0.5f), new TextureCoord(0.0f, 1.0f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, 0.5f), new TextureCoord(0.0f, 0.0f)),
        new Vertex(new VertexPosition(-0.5f, 0.5f, 0.5f), new TextureCoord(1.0f, 0.0f)),
        new Vertex(new VertexPosition(-0.5f, 0.5f, -0.5f), new TextureCoord(1.0f, 1.0f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, -0.5f), new TextureCoord(0.0f, 1.0f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, -0.5f), new TextureCoord(0.0f, 1.0f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, 0.5f), new TextureCoord(0.0f, 0.0f)),
        new Vertex(new VertexPosition(-0.5f, 0.5f, 0.5f), new TextureCoord(1.0f, 0.0f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, 0.5f), new TextureCoord(1.0f, 0.0f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, -0.5f), new TextureCoord(1.0f, 1.0f)),
        new Vertex(new VertexPosition(0.5f, -0.5f, -0.5f), new TextureCoord(0.0f, 1.0f)),
        new Vertex(new VertexPosition(0.5f, -0.5f, -0.5f), new TextureCoord(0.0f, 1.0f)),
        new Vertex(new VertexPosition(0.5f, -0.5f, 0.5f), new TextureCoord(0.0f, 0.0f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, 0.5f), new TextureCoord(1.0f, 0.0f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, -0.5f), new TextureCoord(0.0f, 1.0f)),
        new Vertex(new VertexPosition(0.5f, -0.5f, -0.5f), new TextureCoord(1.0f, 1.0f)),
        new Vertex(new VertexPosition(0.5f, -0.5f, 0.5f), new TextureCoord(1.0f, 0.0f)),
        new Vertex(new VertexPosition(0.5f, -0.5f, 0.5f), new TextureCoord(1.0f, 0.0f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, 0.5f), new TextureCoord(0.0f, 0.0f)),
        new Vertex(new VertexPosition(-0.5f, -0.5f, -0.5f), new TextureCoord(0.0f, 1.0f)),
        new Vertex(new VertexPosition(-0.5f, 0.5f, -0.5f), new TextureCoord(0.0f, 1.0f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, -0.5f), new TextureCoord(1.0f, 1.0f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, 0.5f), new TextureCoord(1.0f, 0.0f)),
        new Vertex(new VertexPosition(0.5f, 0.5f, 0.5f), new TextureCoord(1.0f, 0.0f)),
        new Vertex(new VertexPosition(-0.5f, 0.5f, 0.5f), new TextureCoord(0.0f, 0.0f)),
        new Vertex(new VertexPosition(-0.5f, 0.5f, -0.5f), new TextureCoord(0.0f, 1.0f))
    };

    private static readonly Transform[] cubeTransforms =
    {
        new Transform { Translation = new Vector3D<float>(0.0f, 0.0f, 0.0f) },
        new Transform { Translation = new Vector3D<float>(2.0f, 5.0f, -15.0f) },
        new Transform { Translation = new Vector3D<float>(-1.5f, -2.2f, -2.5f) },
        new Transform { Translation = new Vector3D<float>(-3.8f, -2.0f, -12.3f) },
        new Transform { Translation = new Vector3D<float>(2.4f, -0.4f, -3.5f) },
        new Transform { Translation = new Vector3D<float>(-1.7f, 3.0f, -7.5f) },
        new Transform { Translation = new Vector3D<float>(1.3f, -2.0f, -2.5f) },
        new Transform { Translation = new Vector3D<float>(1.5f, 2.0f, -2.5f) },
        new Transform { Translation = new Vector3D<float>(1.5f, 0.2f, -1.5f) },
        new Transform { Translation = new Vector3D<float>(-1.3f, 1.0f, -1.5f) },
    };

    private static VertexArrayObject<Vertex, uint> vao;
    private static BufferObject<Vertex> vbo;
    private static ShaderProgram shader;
    private static Texture texture;
    private static bool polygonModeToggle = false;

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

        vao = new VertexArrayObject<Vertex, uint>(Gl, vbo);
        vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float);
        vao.VertexAttributePointer(
            1,
            2,
            VertexAttribPointerType.Float,
            offset: sizeof(VertexPosition)
        );

        shader = ShaderProgram.FromFiles(Gl, "shader.vs", "shader.fs");

        texture = Texture.FromFile(Gl, "wall.jpg");
        shader.Set("texture1", 0);

        Gl.Enable(EnableCap.DepthTest);

        var view = Matrix4X4.CreateTranslation(0.0f, 0.0f, -3.0f);
        var projection = Matrix4X4.CreatePerspectiveFieldOfView(
            45.0f.ToRadians(),
            (float)window.Size.X / window.Size.Y,
            0.1f,
            100.0f
        );
        shader.Set("view", view);
        shader.Set("projection", projection);
    }

    private static void OnUpdate(double deltaTime)
    {
        for (int i = 0; i < cubeTransforms.Length; i++)
        {
            float angle = 20.0f * i;
            cubeTransforms[i].Rotation = Quaternion<float>.CreateFromAxisAngle(
                Vector3D.Normalize(new Vector3D<float>(1.0f, 0.3f, 0.5f)),
                angle.ToRadians()
            );
        }
    }

    private static unsafe void OnRender(double deltaTime)
    {
        Gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        vao.Bind();
        shader.Use();
        texture.Use(TextureUnit.Texture0);

        foreach (var cubeTransform in cubeTransforms)
        {
            shader.Set("model", cubeTransform.Matrix);
            Gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)vertices.Length);
        }
    }

    private static void OnClose()
    {
        vao.Dispose();
        vbo.Dispose();
        shader.Dispose();
        texture.Dispose();
    }

    private static void OnResize(Vector2D<int> size) => Gl.Viewport(size);

    private static void Main()
    {
        window.Run();
    }
}
