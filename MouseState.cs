#nullable disable

using System.Numerics;
using Silk.NET.Input;

internal class MouseState
{
    private readonly HashSet<MouseButton> buttonsDown = new();

    public Vector2? Position { get; private set; } = null;
    private List<ScrollDirection> scrollEvents = new();

    public void ResetMouse() => Position = null;

    public void MouseMove(Vector2 position) => Position = position;

    public void MouseScroll(ScrollDirection scroll) => scrollEvents.Add(scroll);

    public IList<ScrollDirection> HandleScrollEvents()
    {
        var events = scrollEvents;
        scrollEvents = new();

        return events;
    }

    public void ButtonDown(MouseButton button) => buttonsDown.Add(button);

    public void ButtonUp(MouseButton button) => buttonsDown.Remove(button);

    public bool IsButtonDown(MouseButton button) => buttonsDown.Contains(button);
}

internal enum ScrollDirection
{
    Up,
    Down
}
