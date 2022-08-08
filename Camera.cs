#nullable disable

using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;

internal class Camera
{
    private const float MovementSpeed = 2.5f;
    private const float MouseSensitivity = 0.05f;

    public Vector3D<float> Position { get; private set; }
    public Vector3D<float> Front { get; private set; } = new(0, 0, -1);
    private static readonly Vector3D<float> up = new(0f, 1f, 0f);

    private float yaw = -90f;
    private float pitch;
    private Vector2? lastMousePosition = null;

    private float fov = 45f;

    private Vector2D<int> _windowSize;
    public Vector2D<int> WindowSize
    {
        get => _windowSize;
        set
        {
            _windowSize = value;
            ResetMouse();
        }
    }

    public Camera(Vector2D<int> windowSize) : this(windowSize, new(0, 0, 3)) { }

    public Camera(Vector2D<int> windowSize, Vector3D<float> position)
    {
        WindowSize = windowSize;
        this.Position = position;
    }

    public Matrix4X4<float> ViewMatrix => Matrix4X4.CreateLookAt(Position, Position + Front, up);

    public Matrix4X4<float> ProjectionMatrix =>
        Matrix4X4.CreatePerspectiveFieldOfView(
            fov.ToRadians(),
            (float)WindowSize.X / WindowSize.Y,
            0.1f,
            100f
        );

    public void ResetMouse() => lastMousePosition = null;

    public void Update(double delta, KeyState keyState, MouseState mouseState)
    {
        UpdateMouse(mouseState);
        UpdateKeys(delta, keyState);
    }

    private void UpdateMouse(MouseState mouseState)
    {
        if (mouseState.Position.HasValue)
        {
            var mousePosition = mouseState.Position.Value;

            lastMousePosition ??= mousePosition;
            Vector2 offset = mousePosition - lastMousePosition.Value;
            offset *= MouseSensitivity;

            yaw += offset.X;
            pitch = (pitch - offset.Y).Clamp(-89f, 89f);
            lastMousePosition = mousePosition;

            var direction = new Vector3D<float>(
                MathF.Cos(yaw.ToRadians()) * MathF.Cos(pitch.ToRadians()),
                MathF.Sin(pitch.ToRadians()),
                MathF.Sin(yaw.ToRadians()) * MathF.Cos(pitch.ToRadians())
            );
            Front = Vector3D.Normalize(direction);
        }

        foreach (var scrollEvent in mouseState.HandleScrollEvents())
        {
            fov = (fov + (scrollEvent == ScrollDirection.Up ? -1 : 1)).Clamp(1f, 45f);
        }
    }

    private void UpdateKeys(double delta, KeyState keyState)
    {
        float speed = MovementSpeed * (float)delta;
        int frontFactor =
            (keyState.IsKeyDown(Key.W) ? 1 : 0) + (keyState.IsKeyDown(Key.S) ? -1 : 0);
        int sideFactor = (keyState.IsKeyDown(Key.D) ? 1 : 0) + (keyState.IsKeyDown(Key.A) ? -1 : 0);
        int upFactor =
            (keyState.IsKeyDown(Key.ShiftLeft) ? 1 : 0)
            + (keyState.IsKeyDown(Key.ControlLeft) ? -1 : 0);

        Position += speed * Front * frontFactor;
        Position += speed * Vector3D.Normalize(Vector3D.Cross(Front, up)) * sideFactor;
        Position += speed * up * upFactor;
    }
}
