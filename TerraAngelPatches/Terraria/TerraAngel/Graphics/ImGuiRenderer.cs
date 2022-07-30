using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.GameInput;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using System.Reflection;

namespace TerraAngel.Graphics
{
    public static class DrawVertDeclaration
    {
        public static readonly VertexDeclaration Declaration;

        public static readonly int Size;

        static DrawVertDeclaration()
        {
            unsafe { Size = sizeof(ImDrawVert); }

            Declaration = new VertexDeclaration(
                Size,

                // Position
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),

                // UV
                new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),

                // Color
                new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            );
        }
    }
    public class TerraImGuiRenderer
    {
        private Game _game;

        // Graphics
        public GraphicsDevice GraphicsDevice;

        public BasicEffect ImGuiShader;
        private RasterizerState RasterizerState;

        private byte[] VertexData;
        private VertexBuffer VertexBuffer;
        private int VertexBufferSize;

        private byte[] IndexData;
        private IndexBuffer IndexBuffer;
        private int IndexBufferSize;

        // Textures
        public Dictionary<IntPtr, Texture2D> LoadedTextures;

        private long TextureId;
        public IntPtr? FontTextureId;

        // Input
        private int ScrollWheelValue;
        private List<int> keyRemappings = new List<int>();

        // DeltaTime
        private DateTime lastTime = DateTime.UtcNow;

        public TerraImGuiRenderer(Game game)
        {
            SetupContext();
            SetupGraphics(game);
            SetupInput();
        }

        protected virtual void SetupContext()
        {
            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
        }
        protected virtual void SetupGraphics(Game game)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            GraphicsDevice = game.GraphicsDevice;

            LoadedTextures = new Dictionary<IntPtr, Texture2D>();

            LoadedTextures.Add(IntPtr.Zero, null);
            TextureId = 1;

            RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.None,
                DepthBias = 0,
                FillMode = FillMode.Solid,
                MultiSampleAntiAlias = false,
                ScissorTestEnable = true,
                SlopeScaleDepthBias = 0
            };

            ImGuiShader = new BasicEffect(GraphicsDevice);
        }
        protected virtual void SetupInput()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.Tab] = (int)Microsoft.Xna.Framework.Input.Keys.Tab);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Microsoft.Xna.Framework.Input.Keys.Left);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Microsoft.Xna.Framework.Input.Keys.Right);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Microsoft.Xna.Framework.Input.Keys.Up);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Microsoft.Xna.Framework.Input.Keys.Down);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.PageUp] = (int)Microsoft.Xna.Framework.Input.Keys.PageUp);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.PageDown] = (int)Microsoft.Xna.Framework.Input.Keys.PageDown);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.Home] = (int)Microsoft.Xna.Framework.Input.Keys.Home);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.End] = (int)Microsoft.Xna.Framework.Input.Keys.End);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.Delete] = (int)Microsoft.Xna.Framework.Input.Keys.Delete);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.Backspace] = (int)Microsoft.Xna.Framework.Input.Keys.Back);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.Enter] = (int)Microsoft.Xna.Framework.Input.Keys.Enter);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.Escape] = (int)Microsoft.Xna.Framework.Input.Keys.Escape);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.Space] = (int)Microsoft.Xna.Framework.Input.Keys.Space);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.A] = (int)Microsoft.Xna.Framework.Input.Keys.A);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.C] = (int)Microsoft.Xna.Framework.Input.Keys.C);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.V] = (int)Microsoft.Xna.Framework.Input.Keys.V);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.X] = (int)Microsoft.Xna.Framework.Input.Keys.X);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.Y] = (int)Microsoft.Xna.Framework.Input.Keys.Y);
            keyRemappings.Add(io.KeyMap[(int)ImGuiKey.Z] = (int)Microsoft.Xna.Framework.Input.Keys.Z);

            io.Fonts.AddFontDefault();
            Client.ClientAssets.LoadFonts(ImGui.GetIO());
            ImGuiStylePtr style = ImGui.GetStyle();
            RangeAccessor<System.Numerics.Vector4> colors = style.Colors;

            colors[(int)ImGuiCol.Text] = new System.Numerics.Vector4(1.00f, 1.00f, 1.00f, 1.00f);
            colors[(int)ImGuiCol.TextDisabled] = new System.Numerics.Vector4(0.50f, 0.50f, 0.50f, 1.00f);
            colors[(int)ImGuiCol.WindowBg] = new System.Numerics.Vector4(0.10f, 0.10f, 0.10f, 1.00f);
            colors[(int)ImGuiCol.ChildBg] = new System.Numerics.Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.PopupBg] = new System.Numerics.Vector4(0.19f, 0.19f, 0.19f, 0.92f);
            colors[(int)ImGuiCol.Border] = new System.Numerics.Vector4(0.19f, 0.19f, 0.19f, 0.29f);
            colors[(int)ImGuiCol.BorderShadow] = new System.Numerics.Vector4(0.00f, 0.00f, 0.00f, 0.24f);
            colors[(int)ImGuiCol.FrameBg] = new System.Numerics.Vector4(0.05f, 0.05f, 0.05f, 0.54f);
            colors[(int)ImGuiCol.FrameBgHovered] = new System.Numerics.Vector4(0.19f, 0.19f, 0.19f, 0.54f);
            colors[(int)ImGuiCol.FrameBgActive] = new System.Numerics.Vector4(0.20f, 0.22f, 0.23f, 1.00f);
            colors[(int)ImGuiCol.TitleBg] = new System.Numerics.Vector4(0.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.TitleBgActive] = new System.Numerics.Vector4(0.06f, 0.06f, 0.06f, 1.00f);
            colors[(int)ImGuiCol.TitleBgCollapsed] = new System.Numerics.Vector4(0.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.MenuBarBg] = new System.Numerics.Vector4(0.14f, 0.14f, 0.14f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarBg] = new System.Numerics.Vector4(0.05f, 0.05f, 0.05f, 0.54f);
            colors[(int)ImGuiCol.ScrollbarGrab] = new System.Numerics.Vector4(0.34f, 0.34f, 0.34f, 0.54f);
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = new System.Numerics.Vector4(0.40f, 0.40f, 0.40f, 0.54f);
            colors[(int)ImGuiCol.ScrollbarGrabActive] = new System.Numerics.Vector4(0.56f, 0.56f, 0.56f, 0.54f);
            colors[(int)ImGuiCol.CheckMark] = new System.Numerics.Vector4(0.33f, 0.67f, 0.86f, 1.00f);
            colors[(int)ImGuiCol.SliderGrab] = new System.Numerics.Vector4(0.34f, 0.34f, 0.34f, 0.54f);
            colors[(int)ImGuiCol.SliderGrabActive] = new System.Numerics.Vector4(0.56f, 0.56f, 0.56f, 0.54f);
            colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0.05f, 0.05f, 0.05f, 0.54f);
            colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(0.19f, 0.19f, 0.19f, 0.54f);
            colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(0.20f, 0.22f, 0.23f, 1.00f);
            colors[(int)ImGuiCol.Header] = new System.Numerics.Vector4(0.00f, 0.00f, 0.00f, 0.52f);
            colors[(int)ImGuiCol.HeaderHovered] = new System.Numerics.Vector4(0.00f, 0.00f, 0.00f, 0.36f);
            colors[(int)ImGuiCol.HeaderActive] = new System.Numerics.Vector4(0.20f, 0.22f, 0.23f, 0.33f);
            colors[(int)ImGuiCol.Separator] = new System.Numerics.Vector4(0.28f, 0.28f, 0.28f, 0.29f);
            colors[(int)ImGuiCol.SeparatorHovered] = new System.Numerics.Vector4(0.44f, 0.44f, 0.44f, 0.29f);
            colors[(int)ImGuiCol.SeparatorActive] = new System.Numerics.Vector4(0.40f, 0.44f, 0.47f, 1.00f);
            colors[(int)ImGuiCol.ResizeGrip] = new System.Numerics.Vector4(0.28f, 0.28f, 0.28f, 0.29f);
            colors[(int)ImGuiCol.ResizeGripHovered] = new System.Numerics.Vector4(0.44f, 0.44f, 0.44f, 0.29f);
            colors[(int)ImGuiCol.ResizeGripActive] = new System.Numerics.Vector4(0.40f, 0.44f, 0.47f, 1.00f);
            colors[(int)ImGuiCol.Tab] = new System.Numerics.Vector4(0.00f, 0.00f, 0.00f, 0.52f);
            colors[(int)ImGuiCol.TabHovered] = new System.Numerics.Vector4(0.14f, 0.14f, 0.14f, 1.00f);
            colors[(int)ImGuiCol.TabActive] = new System.Numerics.Vector4(0.20f, 0.20f, 0.20f, 0.36f);
            colors[(int)ImGuiCol.TabUnfocused] = new System.Numerics.Vector4(0.00f, 0.00f, 0.00f, 0.52f);
            colors[(int)ImGuiCol.TabUnfocusedActive] = new System.Numerics.Vector4(0.14f, 0.14f, 0.14f, 1.00f);
            colors[(int)ImGuiCol.DockingPreview] = new System.Numerics.Vector4(0.33f, 0.67f, 0.86f, 1.00f);
            colors[(int)ImGuiCol.DockingEmptyBg] = new System.Numerics.Vector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotLines] = new System.Numerics.Vector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotLinesHovered] = new System.Numerics.Vector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogram] = new System.Numerics.Vector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogramHovered] = new System.Numerics.Vector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.TableHeaderBg] = new System.Numerics.Vector4(0.00f, 0.00f, 0.00f, 0.52f);
            colors[(int)ImGuiCol.TableBorderStrong] = new System.Numerics.Vector4(0.00f, 0.00f, 0.00f, 0.52f);
            colors[(int)ImGuiCol.TableBorderLight] = new System.Numerics.Vector4(0.28f, 0.28f, 0.28f, 0.29f);
            colors[(int)ImGuiCol.TableRowBg] = new System.Numerics.Vector4(0.00f, 0.00f, 0.00f, 0.00f);
            colors[(int)ImGuiCol.TableRowBgAlt] = new System.Numerics.Vector4(1.00f, 1.00f, 1.00f, 0.06f);
            colors[(int)ImGuiCol.TextSelectedBg] = new System.Numerics.Vector4(0.20f, 0.22f, 0.23f, 1.00f);
            colors[(int)ImGuiCol.DragDropTarget] = new System.Numerics.Vector4(0.33f, 0.67f, 0.86f, 1.00f);
            colors[(int)ImGuiCol.NavHighlight] = new System.Numerics.Vector4(1.00f, 0.00f, 0.00f, 1.00f);
            colors[(int)ImGuiCol.NavWindowingHighlight] = new System.Numerics.Vector4(1.00f, 0.00f, 0.00f, 0.70f);
            colors[(int)ImGuiCol.NavWindowingDimBg] = new System.Numerics.Vector4(1.00f, 0.00f, 0.00f, 0.20f);
            colors[(int)ImGuiCol.ModalWindowDimBg] = new System.Numerics.Vector4(1.00f, 0.00f, 0.00f, 0.35f);
        }

        public virtual IntPtr BindTexture(Texture2D texture)
        {
            IntPtr id = new IntPtr(TextureId++);

            LoadedTextures.Add(id, texture);

            return id;
        }
        public virtual void UnbindTexture(IntPtr textureId)
        {
            LoadedTextures.Remove(textureId);
        }

        public virtual void BeforeLayout(GameTime gameTime)
        {
            ImGui.GetIO().DeltaTime = (float)(DateTime.UtcNow - lastTime).TotalSeconds;
            lastTime = DateTime.UtcNow;
            UpdateInput(gameTime);
            ImGui.NewFrame();
        }
        public virtual void AfterLayout()
        {
            ImGui.Render();
            unsafe { RenderDrawData(ImGui.GetDrawData()); }
        }

        public virtual unsafe void RebuildFontAtlas()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);

            byte[] pixels = new byte[width * height * bytesPerPixel];
            unsafe { Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length); }

            Texture2D tex2d = new Texture2D(GraphicsDevice, width, height, false, SurfaceFormat.Color);
            tex2d.SetData(pixels);

            if (FontTextureId.HasValue) UnbindTexture(FontTextureId.Value);

            FontTextureId = BindTexture(tex2d);

            io.Fonts.SetTexID(FontTextureId.Value);
            io.Fonts.ClearTexData();
        }
        protected virtual BasicEffect UpdateEffect(Texture2D texture, IntPtr specialEffect)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            ImGuiShader.World = Matrix.Identity;
            ImGuiShader.View = Matrix.Identity;
            ImGuiShader.Projection = Matrix.CreateOrthographicOffCenter(0f, io.DisplaySize.X, io.DisplaySize.Y, 0f, -1f, 1f);
            ImGuiShader.TextureEnabled = true;
            ImGuiShader.Texture = texture;
            ImGuiShader.VertexColorEnabled = true;

            return ImGuiShader;
        }
        protected virtual void UpdateInput(GameTime gameTime)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            MouseState mouse = Mouse.GetState();
            KeyboardState keyboard = Keyboard.GetState();

            for (int i = 0; i < 256; i++)
            {
                io.KeysDown[i] = keyboard.IsKeyDown((Keys)i);
            }

            for (int i = 0; i < keyRemappings.Count; i++)
            {
                io.KeysDown[keyRemappings[i]] = keyboard.IsKeyDown((Keys)keyRemappings[i]);
            }




            io.KeyShift = (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) || keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift)) && _game.IsActive;
            io.KeyCtrl = (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl) || keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightControl)) && _game.IsActive;
            io.KeyAlt = (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt) || keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightAlt)) && _game.IsActive;
            io.KeySuper = (keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftWindows) || keyboard.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightWindows)) && _game.IsActive;

            io.DisplaySize = new System.Numerics.Vector2(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            io.DisplayFramebufferScale = new System.Numerics.Vector2(1f, 1f);

            io.MousePos = new System.Numerics.Vector2(mouse.X, mouse.Y);

            io.MouseDown[0] = mouse.LeftButton == ButtonState.Pressed && _game.IsActive;
            io.MouseDown[1] = mouse.RightButton == ButtonState.Pressed && _game.IsActive;
            io.MouseDown[2] = mouse.MiddleButton == ButtonState.Pressed && _game.IsActive;
            int scrollDelta = mouse.ScrollWheelValue - ScrollWheelValue;
            io.MouseWheel = scrollDelta > 0 ? 1 : scrollDelta < 0 ? -1 : 0;
            ScrollWheelValue = mouse.ScrollWheelValue;
        }

        private void RenderDrawData(ImDrawDataPtr drawData)
        {
            // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, vertex/texcoord/color pointers
            Viewport lastViewport = GraphicsDevice.Viewport;
            Rectangle lastScissorBox = GraphicsDevice.ScissorRectangle;

            GraphicsDevice.BlendFactor = Color.White;
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
            GraphicsDevice.RasterizerState = RasterizerState;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            // Handle cases of screen coordinates != from framebuffer coordinates (e.g. retina displays)
            drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

            // Setup projection
            GraphicsDevice.Viewport = new Viewport(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);

            UpdateBuffers(drawData);

            RenderCommandLists(drawData);

            // Restore modified state
            GraphicsDevice.Viewport = lastViewport;
            GraphicsDevice.ScissorRectangle = lastScissorBox;
        }
        private unsafe void UpdateBuffers(ImDrawDataPtr drawData)
        {
            if (drawData.TotalVtxCount == 0)
            {
                return;
            }

            // Expand buffers if we need more room
            if (drawData.TotalVtxCount > VertexBufferSize)
            {
                VertexBuffer?.Dispose();

                VertexBufferSize = (int)(drawData.TotalVtxCount * 1.5f);
                VertexBuffer = new VertexBuffer(GraphicsDevice, DrawVertDeclaration.Declaration, VertexBufferSize, BufferUsage.None);
                VertexData = new byte[VertexBufferSize * DrawVertDeclaration.Size];
            }

            if (drawData.TotalIdxCount > IndexBufferSize)
            {
                IndexBuffer?.Dispose();

                IndexBufferSize = (int)(drawData.TotalIdxCount * 1.5f);
                IndexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, IndexBufferSize, BufferUsage.None);
                IndexData = new byte[IndexBufferSize * sizeof(ushort)];
            }

            // Copy ImGui's vertices and indices to a set of managed byte arrays
            int vtxOffset = 0;
            int idxOffset = 0;

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdListsRange[n];

                fixed (void* vtxDstPtr = &VertexData[vtxOffset * DrawVertDeclaration.Size])
                fixed (void* idxDstPtr = &IndexData[idxOffset * sizeof(ushort)])
                {
                    Buffer.MemoryCopy((void*)cmdList.VtxBuffer.Data, vtxDstPtr, VertexData.Length, cmdList.VtxBuffer.Size * DrawVertDeclaration.Size);
                    Buffer.MemoryCopy((void*)cmdList.IdxBuffer.Data, idxDstPtr, IndexData.Length, cmdList.IdxBuffer.Size * sizeof(ushort));
                }

                vtxOffset += cmdList.VtxBuffer.Size;
                idxOffset += cmdList.IdxBuffer.Size;
            }

            // Copy the managed byte arrays to the gpu vertex- and index buffers
            VertexBuffer.SetData(VertexData, 0, drawData.TotalVtxCount * DrawVertDeclaration.Size);
            IndexBuffer.SetData(IndexData, 0, drawData.TotalIdxCount * sizeof(ushort));
        }
        private unsafe void RenderCommandLists(ImDrawDataPtr drawData)
        {
            GraphicsDevice.SetVertexBuffer(VertexBuffer);
            GraphicsDevice.Indices = IndexBuffer;

            int vtxOffset = 0;
            int idxOffset = 0;

            for (int i = 0; i < drawData.CmdListsCount; i++)
            {
                ImDrawListPtr cmdList = drawData.CmdListsRange[i];

                for (int j = 0; j < cmdList.CmdBuffer.Size; j++)
                {
                    ImDrawCmdPtr drawCmd = cmdList.CmdBuffer[j];


                    if (!LoadedTextures.TryGetValue(drawCmd.TextureId, out Texture2D? cmdTexture))
                    {
                        cmdTexture = GraphicsUtility.MissingTexture;
                        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    }

                    GraphicsDevice.ScissorRectangle = new Rectangle(
                        (int)drawCmd.ClipRect.X,
                        (int)drawCmd.ClipRect.Y,
                        (int)(drawCmd.ClipRect.Z - drawCmd.ClipRect.X),
                        (int)(drawCmd.ClipRect.W - drawCmd.ClipRect.Y)
                    );

                    Effect effect = UpdateEffect(cmdTexture, drawCmd.TextureId);

                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        GraphicsDevice.DrawIndexedPrimitives(
                            primitiveType: PrimitiveType.TriangleList,
                            baseVertex: vtxOffset,
                            minVertexIndex: 0,
                            numVertices: cmdList.VtxBuffer.Size,
                            startIndex: idxOffset,
                            primitiveCount: (int)drawCmd.ElemCount / 3
                        );
                    }

                    idxOffset += (int)drawCmd.ElemCount;
                }

                vtxOffset += cmdList.VtxBuffer.Size;
            }
        }
    }
}
