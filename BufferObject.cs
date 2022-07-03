using Silk.NET.OpenGL;

internal class BufferObject<TDataType> : IDisposable where TDataType : unmanaged
{
    private readonly GL Gl;
    private readonly uint handle;
    private readonly BufferTargetARB bufferType;

    public BufferObject(GL Gl, Span<TDataType> data, BufferTargetARB bufferType)
    {
        this.Gl = Gl;
        this.bufferType = bufferType;

        handle = Gl.GenBuffer();
        Bind();
        unsafe
        {
            Gl.BufferData<TDataType>(
                bufferType,
                (nuint)(data.Length * sizeof(TDataType)),
                data,
                BufferUsageARB.StaticDraw
            );
        }
    }

    public void Bind() => Gl.BindBuffer(bufferType, handle);

    public void Dispose() => Gl.DeleteBuffer(handle);
}
