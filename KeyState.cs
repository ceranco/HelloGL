#nullable disable

using Silk.NET.Input;

internal class KeyState
{
    private readonly HashSet<Key> keysDown = new();

    public void KeyDown(Key key) => keysDown.Add(key);

    public void KeyUp(Key key) => keysDown.Remove(key);

    public bool IsKeyDown(Key key) => keysDown.Contains(key);

    public void Reset() => keysDown.Clear();
}
