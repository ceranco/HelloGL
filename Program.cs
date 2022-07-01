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
        -0.5f,
        -0.5f,
        0.0f,
        0.5f,
        -0.5f,
        0.0f,
        0.0f,
        0.5f,
        0.0f
    };

    private static uint vao;
    private static uint vbo;
    private static uint shader;

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

        uint vertexShader = Gl.CreateShader(ShaderType.VertexShader);
        Gl.ShaderSource(vertexShader, vertexShaderSource);
        Gl.CompileShader(vertexShader);

        string infoLog = Gl.GetShaderInfoLog(vertexShader);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            Console.WriteLine($"Error compiling vertex shader: {infoLog}");
            return;
        }

        uint fragmentShader = Gl.CreateShader(ShaderType.FragmentShader);
        Gl.ShaderSource(fragmentShader, fragmentshaderSource);
        Gl.CompileShader(fragmentShader);

        infoLog = Gl.GetShaderInfoLog(vertexShader);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            Console.WriteLine($"Error compiling fragment shader: {infoLog}");
            return;
        }

        shader = Gl.CreateProgram();
        Gl.AttachShader(shader, vertexShader);
        Gl.AttachShader(shader, fragmentShader);
        Gl.LinkProgram(shader);

        infoLog = Gl.GetProgramInfoLog(shader);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            Console.WriteLine($"Error linking shader: {infoLog}");
            return;
        }

        Gl.DetachShader(shader, vertexShader);
        Gl.DetachShader(shader, fragmentShader);
        Gl.DeleteShader(vertexShader);
        Gl.DeleteShader(fragmentShader);

        Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), null);
        Gl.EnableVertexAttribArray(0);
    }

    private static void OnUpdate(double deltaTime) { }

    private static void OnRender(double deltaTime)
    {
        Gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        Gl.Clear(ClearBufferMask.ColorBufferBit);

        Gl.UseProgram(shader);
        Gl.BindVertexArray(vao);
        Gl.DrawArrays(PrimitiveType.Triangles, 0, 3);
    }

    private static void OnClose() { }

    private static void Main()
    {
        window.Run();
    }
}
