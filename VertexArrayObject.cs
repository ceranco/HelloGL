using Silk.NET.OpenGL;

internal class VertexArrayObject<TVertexType, TIndexType> : IDisposable
    where TVertexType : unmanaged
    where TIndexType : unmanaged
{
    private readonly GL Gl;
    private readonly uint vao;

    public VertexArrayObject(
        GL Gl,
        BufferObject<TVertexType> vbo,
        BufferObject<TIndexType>? ebo = null
    )
    {
        this.Gl = Gl;

        vao = Gl.GenVertexArray();
        Bind();
        vbo.Bind();
        ebo?.Bind();
    }

    public void VertexAttributePointer(
        uint index,
        int size,
        VertexAttribPointerType type,
        uint stride = 1,
        int offset = 0
    )
    {
        Bind();
        unsafe
        {
            Gl.VertexAttribPointer(
                index,
                size,
                type,
                false,
                stride * (uint)sizeof(TVertexType),
                (void*)offset
            );
        }
        Gl.EnableVertexAttribArray(index);
    }

    public void Bind() => Gl.BindVertexArray(vao);

    public void Dispose() => Gl.DeleteVertexArray(vao);
}
