#nullable disable

using System.Numerics;

internal class MouseState
{
    public Vector2? Position { get; private set; } = null;
    private List<ScrollDirection> scrollEvents = new();

    public void MouseMove(Vector2 position) => Position = position;

    public void MouseScroll(ScrollDirection scroll) => scrollEvents.Add(scroll);

    public IList<ScrollDirection> HandleScrollEvents()
    {
        var events = scrollEvents;
        scrollEvents = new();

        return events;
    }
}

internal enum ScrollDirection
{
    Up,
    Down
}
