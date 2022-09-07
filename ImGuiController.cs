using System.Numerics;
using Silk.NET.Windowing;
using Silk.NET.Input;
using ImGuiNET;
using Silk.NET.OpenGL;
using Silk.NET.Maths;

internal class ImGuiController : IDisposable
{
    private readonly IO io;
    private readonly Renderer renderer;

    public ImGuiController(GL Gl, IView view, IInputContext input)
    {
        ImGui.CreateContext();
        ImGui.StyleColorsDark();

        io = new(view, input);
        renderer = new(Gl);
    }

    public void NewFrame(double deltaTime)
    {
        ImGui.NewFrame();
        io.NewFrame(deltaTime);
    }

    public void Render()
    {
        ImGui.Render();
        renderer.Render(ImGui.GetDrawData());
    }

    public void Dispose()
    {
        io.Dispose();
        renderer.Dispose();
        ImGui.DestroyContext();
    }

    private class IO : IDisposable
    {
        private readonly IView view;
        private readonly IInputContext input;
        bool disposed = false;

        public IO(IView view, IInputContext input)
        {
            this.view = view;
            this.input = input;

            var io = ImGui.GetIO();
            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
            io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;

            // TODO: add support for clipboard functions here.
            view.FocusChanged += OnFocusChanged;
            input.Mice[0].MouseMove += OnMouseMove;
            input.Mice[0].MouseDown += OnMouseDown;
            input.Mice[0].MouseUp += OnMouseUp;
            input.Mice[0].Scroll += OnScroll;
            input.Keyboards[0].KeyDown += OnKeyDown;
            input.Keyboards[0].KeyUp += OnKeyUp;
            input.Keyboards[0].KeyChar += OnKeyChar;
        }

        public void NewFrame(double deltaTime)
        {
            var io = ImGui.GetIO();

            // Setup time step
            io.DeltaTime = (float)deltaTime;

            // Setup display size
            var (windowSize, frameBufferSize) = (view.Size, view.FramebufferSize);
            (io.DisplaySize.X, io.DisplaySize.Y) = (windowSize.X, windowSize.Y);
            if (windowSize.X > 0 && windowSize.Y > 0)
            {
                (io.DisplayFramebufferScale.X, io.DisplayFramebufferScale.Y) = (
                    (float)frameBufferSize.X / windowSize.X,
                    (float)frameBufferSize.Y / windowSize.Y
                );
            }

            // Update mouse cursor
            if (
                !io.ConfigFlags.HasFlag(ImGuiConfigFlags.NoMouseCursorChange)
                && !(input.Mice[0].Cursor.CursorMode == CursorMode.Disabled)
            )
            {
                var imguiCursor = ImGui.GetMouseCursor();

                if (imguiCursor == ImGuiMouseCursor.None || io.MouseDrawCursor)
                {
                    // Hide OS mouse cursor if imgui is drawing it or if it wants no cursor
                    input.Mice[0]
                        .Cursor
                        .CursorMode = CursorMode.Hidden;
                }
                else
                {
                    var cursor = imguiCursor.ToStandardCursor();
                    input.Mice[0].Cursor.StandardCursor = cursor;
                }
            }
        }

        public void Dispose()
        {
            disposed = true;

            view.FocusChanged -= OnFocusChanged;
            input.Mice[0].MouseMove -= OnMouseMove;
            input.Mice[0].MouseDown -= OnMouseDown;
            input.Mice[0].MouseUp -= OnMouseUp;
            input.Mice[0].Scroll -= OnScroll;
            input.Keyboards[0].KeyDown -= OnKeyDown;
            input.Keyboards[0].KeyUp -= OnKeyUp;
            input.Keyboards[0].KeyChar -= OnKeyChar;
        }

        private void OnFocusChanged(bool focused)
        {
            if (disposed)
            {
                return;
            }

            var io = ImGui.GetIO();
            io.AddFocusEvent(focused);
        }

        private void OnMouseMove(IMouse mouse, Vector2 position)
        {
            if (disposed || mouse.Cursor.CursorMode == CursorMode.Disabled)
            {
                return;
            }

            var io = ImGui.GetIO();
            io.AddMousePosEvent(position.X, position.Y);
        }

        private void OnMouseDown(IMouse mouse, MouseButton button) =>
            OnMouseButton(mouse, button, true);

        private void OnMouseUp(IMouse mouse, MouseButton button) =>
            OnMouseButton(mouse, button, false);

        private void OnMouseButton(IMouse _, MouseButton button, bool down)
        {
            if (!disposed && button.ToImGuiMouseButton(out var imGuiButton))
            {
                var io = ImGui.GetIO();
                io.AddMouseButtonEvent((int)imGuiButton, down);
            }
        }

        private void OnScroll(IMouse _, ScrollWheel offset)
        {
            if (disposed)
            {
                return;
            }

            var io = ImGui.GetIO();
            io.AddMouseWheelEvent(offset.X, offset.Y);
        }

        private void OnKeyDown(IKeyboard keyboard, Key key, int scancode) =>
            OnKey(keyboard, key, scancode, true);

        private void OnKeyUp(IKeyboard keyboard, Key key, int scancode) =>
            OnKey(keyboard, key, scancode, false);

        private void OnKey(IKeyboard _1, Key key, int _2, bool down)
        {
            if (disposed)
            {
                return;
            }

            var io = ImGui.GetIO();
            var imguiKey = key.ToImGuiKey();
            io.AddKeyEvent(imguiKey, down);
        }

        private void OnKeyChar(IKeyboard _, char @char)
        {
            if (disposed)
            {
                return;
            }

            var io = ImGui.GetIO();
            io.AddInputCharacterUTF16(@char);
        }
    }

    private class Renderer : IDisposable
    {
        private const string VertexShaderSource =
            @"#version 330 core
            layout (location = 0) in vec2 Position;
            layout (location = 1) in vec2 UV;
            layout (location = 2) in vec4 Color;
            uniform mat4 ProjMtx;
            out vec2 Frag_UV;
            out vec4 Frag_Color;
            void main()
            {
                Frag_UV = UV;
                Frag_Color = Color;
                gl_Position = ProjMtx * vec4(Position.xy,0,1);
            }";

        private const string FragmentShaderSource =
            @"#version 330 core
            in vec2 Frag_UV;
            in vec4 Frag_Color;
            uniform sampler2D Texture;
            layout (location = 0) out vec4 Out_Color;
            void main()
            {
                Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
            }";

        private readonly GL Gl;
        private readonly Texture fontTexture;
        private readonly ShaderProgram shader;

        private readonly uint vbo;
        private readonly uint ebo;

        public Renderer(GL Gl)
        {
            this.Gl = Gl;

            var io = ImGui.GetIO();
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

            // Backup GL state
            Gl.GetInteger(GetPName.TextureBinding2D, out int lastTexture);
            Gl.GetInteger(GetPName.ArrayBufferBinding, out int lastArrayBuffer);
            Gl.GetInteger(GetPName.VertexArrayBinding, out int lastVertexArray);

            vbo = Gl.GenBuffer();
            ebo = Gl.GenBuffer();

            shader = new ShaderProgram(Gl, VertexShaderSource, FragmentShaderSource);
            // Create fonts texture
            unsafe
            {
                io.Fonts.GetTexDataAsRGBA32(
                    out byte* pixels,
                    out int width,
                    out int height,
                    out int bytesPerPixel
                );
                fontTexture = new Texture(
                    Gl,
                    new Span<byte>(pixels, width * height * bytesPerPixel),
                    (uint)width,
                    (uint)height,
                    GLEnum.Rgba,
                    minFilter: GLEnum.Linear,
                    magFilter: GLEnum.Linear
                );
            }
            io.Fonts.SetTexID(new IntPtr(fontTexture.ID));

            // Restore modified GL state
            Gl.BindTexture(TextureTarget.Texture2D, (uint)lastTexture);
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, (uint)lastArrayBuffer);
            Gl.BindVertexArray((uint)lastVertexArray);
        }

        public void Render(ImDrawDataPtr drawData)
        {
            // Avoid rendering when minimized, scale coordinates for retin displays
            (int Width, int Height) frameBufferSize = (
                (int)(drawData.DisplaySize.X * drawData.FramebufferScale.X),
                (int)(drawData.DisplaySize.Y * drawData.FramebufferScale.Y)
            );
            if (frameBufferSize.Width < 0 || frameBufferSize.Height < 0)
            {
                return;
            }

            var snapshot = GlState.Snapshot(Gl); // Save GL state

            // Setup GL state
            uint vao = Gl.GenVertexArray();
            SetupRenderState(drawData, frameBufferSize, vao);

            var clipOff = drawData.DisplayPos;
            var clipScale = drawData.FramebufferScale;
            // Render command lists
            unsafe
            {
                for (int n = 0; n < drawData.CmdListsCount; n++)
                {
                    var cmdList = drawData.CmdListsRange[n];

                    // Upload vertex / index buffers
                    nuint vertexBufferSize = (nuint)(cmdList.VtxBuffer.Size * sizeof(ImDrawVert));
                    nuint indexBufferSize = (nuint)(cmdList.IdxBuffer.Size * sizeof(ushort));
                    Gl.BufferData(
                        BufferTargetARB.ArrayBuffer,
                        vertexBufferSize,
                        (void*)cmdList.VtxBuffer.Data,
                        BufferUsageARB.StreamDraw
                    );
                    Gl.BufferData(
                        BufferTargetARB.ElementArrayBuffer,
                        indexBufferSize,
                        (void*)cmdList.IdxBuffer.Data,
                        BufferUsageARB.StreamDraw
                    );

                    for (int i = 0; i < cmdList.CmdBuffer.Size; i++)
                    {
                        var cmd = cmdList.CmdBuffer[i];

                        Vector2 clipMin =
                            new(
                                (cmd.ClipRect.X - clipOff.X) * clipScale.X,
                                (cmd.ClipRect.Y - clipOff.Y) * clipScale.Y
                            );
                        Vector2 clipMax =
                            new(
                                (cmd.ClipRect.Z - clipOff.X) * clipScale.X,
                                (cmd.ClipRect.W - clipOff.Y) * clipScale.Y
                            );
                        if (clipMax.X <= clipMin.X || clipMax.Y <= clipMin.Y)
                        {
                            continue;
                        }

                        // Apply scissor / clipping rectangle (Y is inverted in OpenGL)
                        Gl.Scissor(
                            (int)clipMin.X,
                            (int)(frameBufferSize.Height - clipMax.Y),
                            (uint)(clipMax.X - clipMin.X),
                            (uint)(clipMax.Y - clipMin.Y)
                        );

                        // Bind texture, draw
                        Gl.BindTexture(TextureTarget.Texture2D, (uint)cmd.GetTexID());
                        Gl.DrawElementsBaseVertex(
                            PrimitiveType.Triangles,
                            cmd.ElemCount,
                            DrawElementsType.UnsignedShort,
                            (void*)(cmd.IdxOffset * sizeof(ushort)),
                            (int)cmd.VtxOffset
                        );
                    }
                }
            }

            Gl.DeleteVertexArray(vao);

            snapshot.Restore(); // Restore Gl state
        }

        public void Dispose()
        {
            Gl.DeleteBuffer(vbo);
            Gl.DeleteBuffer(ebo);
            fontTexture.Dispose();
            shader.Dispose();
        }

        private void SetupRenderState(
            ImDrawDataPtr drawData,
            (int Width, int Height) frameBufferSize,
            uint vao
        )
        {
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.Enable(EnableCap.Blend);
            Gl.BlendEquation(BlendEquationModeEXT.FuncAdd);
            Gl.BlendFuncSeparate(
                BlendingFactor.SrcAlpha,
                BlendingFactor.OneMinusSrcAlpha,
                BlendingFactor.One,
                BlendingFactor.OneMinusSrcAlpha
            );
            Gl.Disable(EnableCap.CullFace);
            Gl.Disable(EnableCap.DepthTest);
            Gl.Disable(EnableCap.StencilTest);
            Gl.Enable(EnableCap.ScissorTest);
            Gl.Disable(EnableCap.PrimitiveRestart);
            Gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            // Setup viewport, orthographic projection matrix
            Gl.Viewport(0, 0, (uint)frameBufferSize.Width, (uint)frameBufferSize.Height);
            float L = drawData.DisplayPos.X;
            float R = L + drawData.DisplaySize.X;
            float T = drawData.DisplayPos.Y;
            float B = T + drawData.DisplaySize.Y;
            Matrix4X4<float> ortho =
                new(
                    2.0f / (R - L),
                    0.0f,
                    0.0f,
                    0.0f, //
                    0.0f,
                    2.0f / (T - B),
                    0.0f,
                    0.0f, //
                    0.0f,
                    0.0f,
                    -1.0f,
                    0.0f, //
                    (R + L) / (L - R),
                    (T + B) / (B - T),
                    0.0f,
                    1.0f //
                );
            shader.Set("Texture", 0);
            shader.Set("ProjMtx", ortho);
            Gl.BindSampler(0, 0);

            Gl.BindVertexArray(vao);
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
            Gl.EnableVertexAttribArray(0);
            Gl.EnableVertexAttribArray(1);
            Gl.EnableVertexAttribArray(2);
            unsafe
            {
                Gl.VertexAttribPointer(
                    0,
                    2,
                    GLEnum.Float,
                    false,
                    (uint)sizeof(ImDrawVert),
                    (void*)0
                );
                Gl.VertexAttribPointer(
                    1,
                    2,
                    GLEnum.Float,
                    false,
                    (uint)sizeof(ImDrawVert),
                    (void*)8
                );
                Gl.VertexAttribPointer(
                    2,
                    4,
                    GLEnum.UnsignedByte,
                    true,
                    (uint)sizeof(ImDrawVert),
                    (void*)16
                );
            }
        }

        private struct GlState
        {
            private readonly GL Gl;
            private readonly int lastActiveTexture;
            private readonly int lastProgram;
            private readonly int lastTexture;
            private readonly int lastSampler;
            private readonly int lastArrayBuffer;
            private readonly int lastVao;
            private readonly int[] lastPolygonMode = new int[2];
            private readonly int[] lastViewport = new int[4];
            private readonly int[] lastScissorBox = new int[4];
            private readonly int lastBlendSrcRgb;
            private readonly int lastBlendDstRgb;
            private readonly int lastBlendSrcAlpha;
            private readonly int lastBlendDstAlpha;
            private readonly int lastBlendEquationRgb;
            private readonly int lastBlendEquationAlpha;
            private readonly bool lastEnableBlend;
            private readonly bool lastEnableCullFace;
            private readonly bool lastEnableDepthTest;
            private readonly bool lastEnableStencilTest;
            private readonly bool lastEnableScissorTest;
            private readonly bool lastEnablePrimitiveRestart;

            private GlState(GL Gl)
            {
                this.Gl = Gl;

                Gl.GetInteger(GetPName.ActiveTexture, out lastActiveTexture);
                Gl.GetInteger(GetPName.CurrentProgram, out lastProgram);
                Gl.GetInteger(GetPName.TextureBinding2D, out lastTexture);
                Gl.GetInteger(GetPName.SamplerBinding, out lastSampler);
                Gl.GetInteger(GetPName.ArrayBufferBinding, out lastArrayBuffer);
                Gl.GetInteger(GetPName.VertexArrayBinding, out lastVao);
                Gl.GetInteger(GetPName.PolygonMode, lastPolygonMode);
                Gl.GetInteger(GetPName.Viewport, lastViewport);
                Gl.GetInteger(GetPName.ScissorBox, lastScissorBox);
                Gl.GetInteger(GetPName.BlendSrcRgb, out lastBlendSrcRgb);
                Gl.GetInteger(GetPName.BlendDstRgb, out lastBlendDstRgb);
                Gl.GetInteger(GetPName.BlendSrcAlpha, out lastBlendSrcAlpha);
                Gl.GetInteger(GetPName.BlendDstAlpha, out lastBlendDstAlpha);
                Gl.GetInteger(GetPName.BlendEquationRgb, out lastBlendEquationRgb);
                Gl.GetInteger(GetPName.BlendEquationAlpha, out lastBlendEquationAlpha);
                lastEnableBlend = Gl.IsEnabled(EnableCap.Blend);
                lastEnableCullFace = Gl.IsEnabled(EnableCap.CullFace);
                lastEnableDepthTest = Gl.IsEnabled(EnableCap.DepthTest);
                lastEnableStencilTest = Gl.IsEnabled(EnableCap.StencilTest);
                lastEnableScissorTest = Gl.IsEnabled(EnableCap.ScissorTest);
                lastEnablePrimitiveRestart = Gl.IsEnabled(EnableCap.PrimitiveRestart);
            }

            public static GlState Snapshot(GL Gl) => new(Gl);

            public void Restore()
            {
                Gl.UseProgram((uint)lastProgram);
                Gl.BindTexture(TextureTarget.Texture2D, (uint)lastTexture);
                Gl.BindSampler(0, (uint)lastSampler);
                Gl.ActiveTexture((GLEnum)lastActiveTexture);
                Gl.BindVertexArray((uint)lastVao);
                Gl.BindBuffer(BufferTargetARB.ArrayBuffer, (uint)lastArrayBuffer);
                Gl.BlendEquationSeparate(
                    (GLEnum)lastBlendEquationRgb,
                    (GLEnum)lastBlendEquationAlpha
                );
                Gl.BlendFuncSeparate(
                    (GLEnum)lastBlendSrcRgb,
                    (GLEnum)lastBlendDstRgb,
                    (GLEnum)lastBlendSrcAlpha,
                    (GLEnum)lastBlendDstAlpha
                );
                RestoreCapability(Gl, EnableCap.Blend, lastEnableBlend);
                RestoreCapability(Gl, EnableCap.CullFace, lastEnableCullFace);
                RestoreCapability(Gl, EnableCap.DepthTest, lastEnableDepthTest);
                RestoreCapability(Gl, EnableCap.StencilTest, lastEnableStencilTest);
                RestoreCapability(Gl, EnableCap.ScissorTest, lastEnableScissorTest);
                RestoreCapability(Gl, EnableCap.PrimitiveRestart, lastEnablePrimitiveRestart);
                Gl.PolygonMode(MaterialFace.FrontAndBack, (GLEnum)lastPolygonMode[0]);
                Gl.Viewport(
                    lastViewport[0],
                    lastViewport[1],
                    (uint)lastViewport[2],
                    (uint)lastViewport[3]
                );
                Gl.Scissor(
                    lastScissorBox[0],
                    lastScissorBox[1],
                    (uint)lastScissorBox[2],
                    (uint)lastScissorBox[3]
                );

                static void RestoreCapability(GL Gl, EnableCap capability, bool enable)
                {
                    if (enable)
                    {
                        Gl.Enable(capability);
                    }
                    else
                    {
                        Gl.Disable(capability);
                    }
                }
            }
        }
    }
}

internal static class ImGuiExtensions
{
    public static bool ToImGuiMouseButton(this MouseButton button, out ImGuiMouseButton imGuiButton)
    {
        (bool valid, imGuiButton) = button switch
        {
            MouseButton.Left => (true, ImGuiMouseButton.Left),
            MouseButton.Right => (true, ImGuiMouseButton.Right),
            MouseButton.Middle => (true, ImGuiMouseButton.Middle),
            _ => (false, ImGuiMouseButton.Left)
        };

        return valid;
    }

    public static ImGuiKey ToImGuiKey(this Key key) =>
        key switch
        {
            Key.Space => ImGuiKey.Space,
            Key.Apostrophe => ImGuiKey.Apostrophe,
            Key.Comma => ImGuiKey.Comma,
            Key.Minus => ImGuiKey.Minus,
            Key.Period => ImGuiKey.Period,
            Key.Slash => ImGuiKey.Slash,
            Key.Number0 => ImGuiKey._0,
            Key.Number1 => ImGuiKey._1,
            Key.Number2 => ImGuiKey._2,
            Key.Number3 => ImGuiKey._3,
            Key.Number4 => ImGuiKey._4,
            Key.Number5 => ImGuiKey._5,
            Key.Number6 => ImGuiKey._6,
            Key.Number7 => ImGuiKey._7,
            Key.Number8 => ImGuiKey._8,
            Key.Number9 => ImGuiKey._9,
            Key.Semicolon => ImGuiKey.Semicolon,
            Key.Equal => ImGuiKey.Equal,
            Key.A => ImGuiKey.A,
            Key.B => ImGuiKey.B,
            Key.C => ImGuiKey.C,
            Key.D => ImGuiKey.D,
            Key.E => ImGuiKey.E,
            Key.F => ImGuiKey.F,
            Key.G => ImGuiKey.G,
            Key.H => ImGuiKey.H,
            Key.I => ImGuiKey.I,
            Key.J => ImGuiKey.J,
            Key.K => ImGuiKey.K,
            Key.L => ImGuiKey.L,
            Key.M => ImGuiKey.M,
            Key.N => ImGuiKey.N,
            Key.O => ImGuiKey.O,
            Key.P => ImGuiKey.P,
            Key.Q => ImGuiKey.Q,
            Key.R => ImGuiKey.R,
            Key.S => ImGuiKey.S,
            Key.T => ImGuiKey.T,
            Key.U => ImGuiKey.U,
            Key.V => ImGuiKey.V,
            Key.W => ImGuiKey.W,
            Key.X => ImGuiKey.X,
            Key.Y => ImGuiKey.Y,
            Key.Z => ImGuiKey.Z,
            Key.LeftBracket => ImGuiKey.LeftBracket,
            Key.BackSlash => ImGuiKey.Backslash,
            Key.RightBracket => ImGuiKey.RightBracket,
            Key.GraveAccent => ImGuiKey.GraveAccent,
            Key.Escape => ImGuiKey.Escape,
            Key.Enter => ImGuiKey.Enter,
            Key.Tab => ImGuiKey.Tab,
            Key.Backspace => ImGuiKey.Backspace,
            Key.Insert => ImGuiKey.Insert,
            Key.Delete => ImGuiKey.Delete,
            Key.Right => ImGuiKey.RightArrow,
            Key.Left => ImGuiKey.LeftArrow,
            Key.Down => ImGuiKey.DownArrow,
            Key.Up => ImGuiKey.UpArrow,
            Key.PageUp => ImGuiKey.PageUp,
            Key.PageDown => ImGuiKey.PageDown,
            Key.Home => ImGuiKey.Home,
            Key.End => ImGuiKey.End,
            Key.CapsLock => ImGuiKey.CapsLock,
            Key.ScrollLock => ImGuiKey.ScrollLock,
            Key.NumLock => ImGuiKey.NumLock,
            Key.PrintScreen => ImGuiKey.PrintScreen,
            Key.Pause => ImGuiKey.Pause,
            Key.F1 => ImGuiKey.F1,
            Key.F2 => ImGuiKey.F2,
            Key.F3 => ImGuiKey.F3,
            Key.F4 => ImGuiKey.F4,
            Key.F5 => ImGuiKey.F5,
            Key.F6 => ImGuiKey.F6,
            Key.F7 => ImGuiKey.F7,
            Key.F8 => ImGuiKey.F8,
            Key.F9 => ImGuiKey.F9,
            Key.F10 => ImGuiKey.F10,
            Key.F11 => ImGuiKey.F11,
            Key.F12 => ImGuiKey.F12,
            Key.Keypad0 => ImGuiKey.Keypad0,
            Key.Keypad1 => ImGuiKey.Keypad1,
            Key.Keypad2 => ImGuiKey.Keypad2,
            Key.Keypad3 => ImGuiKey.Keypad3,
            Key.Keypad4 => ImGuiKey.Keypad4,
            Key.Keypad5 => ImGuiKey.Keypad5,
            Key.Keypad6 => ImGuiKey.Keypad6,
            Key.Keypad7 => ImGuiKey.Keypad7,
            Key.Keypad8 => ImGuiKey.Keypad8,
            Key.Keypad9 => ImGuiKey.Keypad9,
            Key.KeypadDecimal => ImGuiKey.KeypadDecimal,
            Key.KeypadDivide => ImGuiKey.KeypadDivide,
            Key.KeypadMultiply => ImGuiKey.KeypadMultiply,
            Key.KeypadSubtract => ImGuiKey.KeypadSubtract,
            Key.KeypadAdd => ImGuiKey.KeypadAdd,
            Key.KeypadEnter => ImGuiKey.KeypadEnter,
            Key.KeypadEqual => ImGuiKey.KeypadEqual,
            Key.ShiftLeft => ImGuiKey.LeftShift,
            Key.ControlLeft => ImGuiKey.LeftCtrl,
            Key.AltLeft => ImGuiKey.LeftAlt,
            Key.SuperLeft => ImGuiKey.LeftSuper,
            Key.ShiftRight => ImGuiKey.RightShift,
            Key.ControlRight => ImGuiKey.RightCtrl,
            Key.AltRight => ImGuiKey.RightAlt,
            Key.SuperRight => ImGuiKey.RightSuper,
            Key.Menu => ImGuiKey.Menu,
            _ => ImGuiKey.None,
        };

    public static StandardCursor ToStandardCursor(this ImGuiMouseCursor cursor) =>
        cursor switch
        {
            ImGuiMouseCursor.Arrow => StandardCursor.Arrow,
            ImGuiMouseCursor.TextInput => StandardCursor.IBeam,
            ImGuiMouseCursor.ResizeAll => StandardCursor.HResize,
            ImGuiMouseCursor.ResizeNS => StandardCursor.VResize,
            ImGuiMouseCursor.ResizeEW => StandardCursor.HResize,
            ImGuiMouseCursor.ResizeNESW => StandardCursor.VResize,
            ImGuiMouseCursor.ResizeNWSE => StandardCursor.VResize,
            ImGuiMouseCursor.Hand => StandardCursor.Hand,
            _ => StandardCursor.Default,
        };
}
