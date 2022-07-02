using System.Runtime.Serialization;

[Serializable]
internal class ShaderError : Exception
{
    public ShaderError() { }

    public ShaderError(string message) : base(message) { }

    public ShaderError(string message, Exception innerException) : base(message, innerException) { }

    protected ShaderError(SerializationInfo info, StreamingContext context) : base(info, context)
    { }
}
