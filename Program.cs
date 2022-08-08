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

    private static readonly DirectionalLight directionalLight =
        new(
            new(-0.2f, -1.0f, -0.3f),
            new Vector3(1.0f) * 0.05f,
            new Vector3(1.0f) * 0.4f,
            new Vector3(1.0f) * 0.5f
        );
    private static readonly PointLight[] pointLights =
    {
        new(
            new(0.7f, 0.2f, 2.0f),
            new Vector3(1.0f) * 0.05f,
            new Vector3(1.0f) * 0.8f,
            new Vector3(1.0f),
            1.0f,
            0.09f,
            0.032f
        ),
        new(
            new(2.3f, -3.3f, -4.0f),
            new Vector3(1.0f) * 0.05f,
            new Vector3(1.0f) * 0.8f,
            new Vector3(1.0f),
            1.0f,
            0.09f,
            0.032f
        ),
        new(
            new(-4.0f, 2.0f, -12.0f),
            new Vector3(1.0f) * 0.05f,
            new Vector3(1.0f) * 0.8f,
            new Vector3(1.0f),
            1.0f,
            0.09f,
            0.032f
        ),
        new(
            new(0.0f, 0.0f, -3.0f),
            new Vector3(1.0f) * 0.05f,
            new Vector3(1.0f) * 0.8f,
            new Vector3(1.0f),
            1.0f,
            0.09f,
            0.032f
        )
    };

    private static readonly Transform[] modelTransforms =
    {
        new() { Translation = new(0.0f, 0.0f, 0.0f) },
        new() { Translation = new(2.0f, 5.0f, -15.0f) },
        new() { Translation = new(-1.5f, -2.2f, -2.5f) },
        new() { Translation = new(-3.8f, -2.0f, -12.3f) },
        new() { Translation = new(2.4f, -0.4f, -3.5f) },
        new() { Translation = new(-1.7f, 3.0f, -7.5f) },
        new() { Translation = new(1.3f, -2.0f, -2.5f) },
        new() { Translation = new(1.5f, 2.0f, -2.5f) },
        new() { Translation = new(1.5f, 0.2f, -1.5f) },
        new() { Translation = new(-1.3f, 1.0f, -1.5f) }
    };

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
        modelShader.Set("directionalLight", directionalLight);
        modelShader.Set("pointLights", pointLights);
        modelShader.Set(
            "material",
            new TexturedMaterial(
                TextureUnit.Texture0,
                TextureUnit.Texture1,
                TextureUnit.Texture2,
                32f
            )
        );

        lightShader = ShaderProgram.FromFiles(Gl, "light.vs", "light.fs");

        Gl.Enable(EnableCap.DepthTest);
    }

    private static void OnUpdate(double deltaTime)
    {
        camera.Update(deltaTime, keyState, mouseState);

        modelShader.Set("view", camera.ViewMatrix);
        modelShader.Set("projection", camera.ProjectionMatrix);
        modelShader.Set("viewPos", camera.Position);

        lightShader.Set("view", camera.ViewMatrix);
        lightShader.Set("projection", camera.ProjectionMatrix);

        for (int i = 0; i < modelTransforms.Length; i++)
        {
            float angle = 20.0f * i;
            modelTransforms[i].Rotation = Quaternion<float>.CreateFromAxisAngle(
                Vector3D.Normalize<float>(new(1.0f, 0.3f, 0.5f)),
                angle.ToRadians()
            );
        }
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

        foreach (var transform in modelTransforms)
        {
            modelShader.Set("model", transform.Matrix);
            Gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)Vertex.Cube.Length);
        }

        lightVao.Bind();
        lightShader.Use();

        foreach (var light in pointLights)
        {
            var scale = Matrix4X4.CreateScale(0.2f);
            var translate = Matrix4X4.CreateTranslation(light.Position.ToVector3D());
            lightShader.Set("model", scale * translate);
            lightShader.Set("lightColor", light.Diffuse);
            Gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)Vertex.Cube.Length);
        }
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
