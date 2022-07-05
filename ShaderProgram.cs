using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

internal class ShaderProgram : IDisposable
{
    private readonly GL Gl;
    private readonly uint shaderProgram;

    public static ShaderProgram FromFiles(GL Gl, string vertexFile, string fragmentFile) =>
        new(Gl, File.ReadAllText(vertexFile), File.ReadAllText(fragmentFile));

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

    public void Set(string name, bool value)
    {
        int location = Gl.GetUniformLocation(shaderProgram, name);
        if (location == -1)
        {
            throw new ShaderError($"Couldn't find uniform: {name}");
        }

        Use();
        Gl.Uniform1(location, value ? 1 : 0);
    }

    public void Set(string name, int value)
    {
        int location = Gl.GetUniformLocation(shaderProgram, name);
        if (location == -1)
        {
            throw new ShaderError($"Couldn't find uniform: {name}");
        }

        Use();
        Gl.Uniform1(location, value);
    }

    public void Set(string name, float value)
    {
        int location = Gl.GetUniformLocation(shaderProgram, name);
        if (location == -1)
        {
            throw new ShaderError($"Couldn't find uniform: {name}");
        }

        Use();
        Gl.Uniform1(location, value);
    }

    public void Set(string name, float x, float y, float z, float w) =>
        Set(name, new Vector4(x, y, z, y));

    public void Set(string name, Vector4 value)
    {
        int location = Gl.GetUniformLocation(shaderProgram, name);
        if (location == -1)
        {
            throw new ShaderError($"Couldn't find uniform: {name}");
        }

        Use();
        Gl.Uniform4(location, ref value);
    }

    public void Set(string name, Matrix4X4<float> value)
    {
        int location = Gl.GetUniformLocation(shaderProgram, name);
        if (location == -1)
        {
            throw new ShaderError($"Couldn't find uniform: {name}");
        }

        Use();
        unsafe
        {
            Gl.UniformMatrix4(location, 1, false, (float*)&value);
        }
    }

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
