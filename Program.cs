#nullable disable

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

internal class Program
{
    private static readonly IWindow window = CreateWindow();
    private static GL Gl;

    private static readonly string vertexShaderSource =
        @"
		#version 330 core
		layout (location = 0) in vec3 aPos;

		void main()
		{
			gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
		}
		";

    private static readonly string fragmentshaderSource =
        @"
		#version 330 core
		out vec4 FragColor;

		void main()
		{
			FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
		}
		";

    private static readonly float[] vertices =
    {
        0.5f,
        0.5f,
        0.0f,
        0.5f,
        -0.5f,
        0.0f,
        -0.5f,
        -0.5f,
        0.0f,
        -0.5f,
        0.5f,
        0.0f
    };

    private static readonly uint[] indices = { 0, 1, 3, 1, 2, 3 };

    private static uint vao;
    private static uint vbo;
    private static uint ebo;
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

        vao = Gl.GenVertexArray();
        Gl.BindVertexArray(vao);

        vbo = Gl.GenBuffer();
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        Gl.BufferData<float>(
            BufferTargetARB.ArrayBuffer,
            (nuint)(vertices.Length * sizeof(float)),
            vertices,
            BufferUsageARB.StaticDraw
        );

        ebo = Gl.GenBuffer();
        Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        Gl.BufferData<uint>(
            BufferTargetARB.ElementArrayBuffer,
            (nuint)(indices.Length * sizeof(uint)),
            indices,
            BufferUsageARB.StaticDraw
        );

        shader = new ShaderProgram(Gl, vertexShaderSource, fragmentshaderSource);
        Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), null);
        Gl.EnableVertexAttribArray(0);
    }

    private static void OnUpdate(double deltaTime) { }

    private static unsafe void OnRender(double deltaTime)
    {
        Gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        Gl.Clear(ClearBufferMask.ColorBufferBit);

        shader.Use();
        Gl.BindVertexArray(vao);
        Gl.DrawElements(
            PrimitiveType.Triangles,
            (uint)indices.Length,
            DrawElementsType.UnsignedInt,
            null
        );
    }

    private static void OnClose()
    {
        Gl.DeleteBuffer(vbo);
        Gl.DeleteBuffer(ebo);
        Gl.DeleteVertexArray(vao);
        shader.Dispose();
    }

    private static void Main()
    {
        window.Run();
    }
}
