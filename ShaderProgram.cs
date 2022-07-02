using Silk.NET.OpenGL;

internal class ShaderProgram : IDisposable
{
    private readonly GL Gl;
    private readonly uint shaderProgram;

    public ShaderProgram(GL Gl, string vertexSource, string fragmentSource)
    {
        this.Gl = Gl;

        uint vertexShader = Gl.CreateShader(ShaderType.VertexShader);
        Gl.ShaderSource(vertexShader, vertexSource);
        Gl.CompileShader(vertexShader);
        CheckShaderCompilation(vertexShader, "Error compiling vertex shader: {0}");

        uint fragmentShader = Gl.CreateShader(ShaderType.FragmentShader);
        Gl.ShaderSource(fragmentShader, fragmentSource);
        Gl.CompileShader(fragmentShader);
        CheckShaderCompilation(vertexShader, "Error compiling fragment shader: {0}");

        shaderProgram = Gl.CreateProgram();
        Gl.AttachShader(shaderProgram, vertexShader);
        Gl.AttachShader(shaderProgram, fragmentShader);
        Gl.LinkProgram(shaderProgram);
        CheckShaderLinking("Error linking shader: {0}");

        Gl.DetachShader(shaderProgram, vertexShader);
        Gl.DetachShader(shaderProgram, fragmentShader);
        Gl.DeleteShader(vertexShader);
        Gl.DeleteShader(fragmentShader);
    }

    public void Use() => Gl.UseProgram(shaderProgram);

    public void Dispose() => Gl.DeleteProgram(shaderProgram);

    private void CheckShaderCompilation(uint shader, string errorMsgFormat)
    {
        string infoLog = Gl.GetShaderInfoLog(shader);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new ShaderError(string.Format(errorMsgFormat, infoLog));
        }
    }

    private void CheckShaderLinking(string errorMsgFormat)
    {
        string infoLog = Gl.GetProgramInfoLog(shaderProgram);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new ShaderError(string.Format(errorMsgFormat, infoLog));
        }
    }
}
