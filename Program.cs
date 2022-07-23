#nullable disable

using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

internal class Program
{
    private static readonly IWindow window = CreateWindow();
    private static readonly KeyState keyState = new();
    private static readonly MouseState mouseState = new();
    private static GL Gl;
    private static readonly Camera camera = new(new(1600, 1600), new(0, 0, 5));

    private static VertexArrayObject<Vertex, uint> modelVao;
    private static VertexArrayObject<Vertex, uint> lightVao;
    private static BufferObject<Vertex> boxVbo;
    private static ShaderProgram modelShader;
    private static ShaderProgram lightShader;
    private static Texture diffuseMap;
    private static Texture specularMap;
    private static Texture emissionMap;
    private static bool polygonModeToggle = false;
    private static int material = 0;

    private static readonly Transform lightTransform = new() { Scale = 0.2f };

    private static IWindow CreateWindow()
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1600, 1600);
        options.Title = "Hello OpenGL with Silk.NET";
        options.PreferredDepthBufferBits = 24;
        options.Samples = 8;
        options.VSync = false;

        var window = Window.Create(options);
        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.Closing += OnClose;
        window.Resize += OnResize;
        window.FocusChanged += OnFocusChanged;
        window.StateChanged += OnStateChanged;

        return window;
    }

    private static void OnStateChanged(WindowState state) => camera.ResetMouse();

    private static void OnFocusChanged(bool obj) => camera.ResetMouse();

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
                    case Key.Right:
                        material = (material + 1) % Material.All.Length;
                        Console.WriteLine($"Material: {Material.All[material].Name}");
                        break;
                    case Key.Left:
                        material = (material - 1) % Material.All.Length;
                        Console.WriteLine($"Material: {Material.All[material].Name}");
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

        diffuseMap = Texture.FromFile(Gl, "container.png");
        specularMap = Texture.FromFile(Gl, "container_specular.png");
        emissionMap = Texture.FromFile(Gl, "matrix.jpg");

        boxVbo = new BufferObject<Vertex>(Gl, Vertex.Cube, BufferTargetARB.ArrayBuffer);

        modelVao = new VertexArrayObject<Vertex, uint>(Gl, boxVbo);
        modelVao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float);
        modelVao.VertexAttributePointer(
            1,
            3,
            VertexAttribPointerType.Float,
            1,
            sizeof(VertexPosition)
        );
        modelVao.VertexAttributePointer(
            2,
            2,
            VertexAttribPointerType.Float,
            1,
            sizeof(VertexPosition) + sizeof(VertexNormal)
        );

        lightVao = new VertexArrayObject<Vertex, uint>(Gl, boxVbo);
        lightVao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float);

        modelShader = ShaderProgram.FromFiles(Gl, "model.vs", "model.fs");
        modelShader.Set("light.ambient", 0.2f, 0.2f, 0.2f);
        modelShader.Set("light.diffuse", 0.5f, 0.5f, 0.5f);
        modelShader.Set("light.specular", 1.0f, 1.0f, 1.0f);

        lightShader = ShaderProgram.FromFiles(Gl, "light.vs", "light.fs");

        Gl.Enable(EnableCap.DepthTest);
    }

    private static void OnUpdate(double deltaTime)
    {
        camera.Update(deltaTime, keyState, mouseState);

        lightTransform.Translation = new(
            MathF.Cos((float)window.Time) * 2f,
            1f,
            MathF.Sin((float)window.Time) * 2f
        );
        Vector3 lightColor =
            new(
                // MathF.Sin((float)(window.Time) * 0.2f),
                // MathF.Sin((float)(window.Time) * 0.7f),
                // MathF.Sin((float)(window.Time) * 1.3f)
                1f,
                1f,
                1f
            );

        modelShader.Set("model", Matrix4X4<float>.Identity);
        modelShader.Set("view", camera.ViewMatrix);
        modelShader.Set("projection", camera.ProjectionMatrix);
        modelShader.Set("light.ambient", lightColor * 0.2f);
        modelShader.Set("light.diffuse", lightColor * 0.5f);
        modelShader.Set("light.position", lightTransform.Translation);
        modelShader.Set("viewPos", camera.Position);
        modelShader.Set(
            "material",
            new TexturedMaterial(
                TextureUnit.Texture0,
                TextureUnit.Texture1,
                TextureUnit.Texture2,
                32f
            )
        // new Material(new(1f, 0.5f, 0.31f), new(1f, 0.5f, 0.31f), new(0.5f, 0.5f, 0.5f), 32f)
        );
        modelShader.Set("time", (float)window.Time);

        lightShader.Set("model", lightTransform.Matrix);
        lightShader.Set("view", camera.ViewMatrix);
        lightShader.Set("projection", camera.ProjectionMatrix);
        lightShader.Set("lightColor", lightColor);
    }

    private static unsafe void OnRender(double deltaTime)
    {
        Gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        Gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        modelVao.Bind();
        modelShader.Use();
        diffuseMap.Use(TextureUnit.Texture0);
        specularMap.Use(TextureUnit.Texture1);
        emissionMap.Use(TextureUnit.Texture2);
        Gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)Vertex.Cube.Length);

        lightVao.Bind();
        lightShader.Use();
        Gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)Vertex.Cube.Length);
    }

    private static void OnClose()
    {
        modelVao.Dispose();
        lightVao.Dispose();
        boxVbo.Dispose();
        modelShader.Dispose();
        lightShader.Dispose();
        diffuseMap.Dispose();
        specularMap.Dispose();
        emissionMap.Dispose();
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
