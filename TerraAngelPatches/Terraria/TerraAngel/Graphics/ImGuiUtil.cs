using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using TerraAngel.Graphics;
using System.Threading;
using ImGuiNET;
using Microsoft.Xna.Framework;
using TerraAngel.Utility;
using Terraria.GameContent.UI;
using NVector2 = System.Numerics.Vector2;


namespace TerraAngel.Graphics
{
    public static class ImGuiUtil
    {
        public static void TextColored(string text, Color color)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, color.PackedValue);
            ImGui.TextUnformatted(text);
            ImGui.PopStyleColor();
        }
        public static void ColorEdit4(string label, ref Color color)
        {
            System.Numerics.Vector4 v4c = color.ToVector4().ToNumerics();
            if (ImGui.ColorEdit4(label, ref v4c))
            {
                color = new Color(v4c.ToXNA());
            }
        }

        public static void DrawTileRect(this ImDrawListPtr drawList, Vector2 startTile, Vector2 endTile, uint col)
        {
            Vector2 min;
            Vector2 max;
            if (Main.mapFullscreen)
            {
                min = Util.WorldToScreenFullscreenMap(startTile * 16f);
                max = Util.WorldToScreenFullscreenMap(endTile * 16f);
            }
            else
            {
                min = Util.WorldToScreen(startTile * 16f);
                max = Util.WorldToScreen(endTile * 16f);
            }
            drawList.AddRectFilled(min.ToNumerics(), max.ToNumerics(), col);
        }

        private static int itemPtr = 0;
        private static int[] ItemsToLoad = new int[ItemID.Count];
        public static void ItemLoaderThread(TerraImGuiRenderer renderer)
        {
            while (true)
            {
                lock (ItemsToLoad)
                {
                    for (int i = 0; i < itemPtr; i++)
                    {
                        Main.instance.LoadItem(ItemsToLoad[i]);
                        ItemImages[ItemsToLoad[i]] = renderer.BindTexture(TextureAssets.Item[ItemsToLoad[i]].Value);
                    }
                    itemPtr = 0;
                }
                Thread.Sleep(32);
            }
        }

        private static IntPtr[] ItemImages = new IntPtr[ItemID.Count];
        public static bool ItemButton(int id, string uid, out Vector2 min, out Vector2 max, out bool visible, bool showTooltip = true, bool clickToCopy = false, int count = 0, bool isSelected = false)
        {
            return ItemButton(id, uid, new Vector2(32, 32), out min, out max, out visible, showTooltip, clickToCopy, count, isSelected);
        }
        public static bool ItemButton(int id, string uid, Vector2 size, out Vector2 buttonMin, out Vector2 buttonMax, out bool visible, bool showTooltip = true, bool clickToCopy = false, int count = 0, bool isSelected = false)
        {
            System.Numerics.Vector2 drawSize = size.ToNumerics();
            ImGuiStylePtr style = ImGui.GetStyle();
            ImGui.PushID(uid);
            bool clicked = false;
            if (ImGui.Button("", drawSize + style.ItemSpacing))
            {
                clicked = true;
                if (clickToCopy)
                {
                    Main.playerInventory = true;
                    if (Main.mouseItem.type == 0)
                    {
                        Main.mouseItem.SetDefaults(id);
                        if (count == 0)
                        {
                            Main.mouseItem.stack = Main.mouseItem.maxStack;
                        }
                        else
                        {
                            Main.mouseItem.stack = count;
                        }
                    }
                }
            }
            System.Numerics.Vector2 min = ImGui.GetItemRectMin();
            System.Numerics.Vector2 max = ImGui.GetItemRectMax();
            ImGui.PopID();

            visible = false;
            if (ImGui.IsRectVisible(min, max))
            {
                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                if (isSelected)
                {
                    drawList.AddRectFilledMultiColor(min, max, 0xFF1de5ff, 0xFFa4ffff, 0xFF1de5ff, 0xFF00b4ff);
                }
                visible = true;
                if (ItemImages[id] == IntPtr.Zero)
                {
                    ItemImages[id] = (IntPtr)(-1);
                    lock (ItemsToLoad)
                    {
                        ItemsToLoad[itemPtr] = id;
                        itemPtr++;
                    }
                }
                if (ItemImages[id] != (IntPtr)(-1))
                {
                    Texture2D value = TextureAssets.Item[id].Value;
                    if (TextureAssets.Item[id].IsLoaded)
                    {
                        bool animated = Main.itemAnimations[id] != null;

                        if (!animated)
                        {
                            int width = value.Width;
                            int height = value.Height;
                            float num = 1f;
                            if ((float)width > drawSize.X || (float)height > drawSize.Y)
                            {
                                num = ((width <= height) ? (drawSize.X / (float)height) : (drawSize.Y / (float)width));
                            }
                            System.Numerics.Vector2 scaledEnd = (new System.Numerics.Vector2(width, height) * num);
                            System.Numerics.Vector2 start = (min + max) / 2 - scaledEnd / 2;
                            drawList.AddImage(ItemImages[id], start, start + scaledEnd);
                        }
                        else
                        {
                            Rectangle animationRect = Main.itemAnimations[id].GetFrame(value);

                            int width = animationRect.Width;
                            int height = animationRect.Height;
                            float num = 1f;
                            if ((float)width > drawSize.X || (float)height > drawSize.Y)
                            {
                                num = ((width <= height) ? (drawSize.X / (float)height) : (drawSize.Y / (float)width));
                            }
                            System.Numerics.Vector2 scaledEnd = (new System.Numerics.Vector2(width, height) * num);
                            System.Numerics.Vector2 start = (min + max) / 2 - scaledEnd / 2;

                            System.Numerics.Vector2 rectStart = new System.Numerics.Vector2(animationRect.X, animationRect.Y);
                            System.Numerics.Vector2 rectEnd = new System.Numerics.Vector2(animationRect.X + width, animationRect.Y + height);

                            System.Numerics.Vector2 uvMin = rectStart / new System.Numerics.Vector2(value.Width, value.Height);
                            System.Numerics.Vector2 uvMax = rectEnd / new System.Numerics.Vector2(value.Width, value.Height);
                            drawList.AddImage(ItemImages[id], start, start + scaledEnd, uvMin, uvMax);

                        }
                    }

                    if (showTooltip)
                    {
                        if (ImGui.IsMouseHoveringRect(min, max))
                        {
                            ImGui.BeginTooltip();
                            Item i = new Item();
                            i.SetDefaults(id);
                            ImGuiUtil.TextColored(Lang.GetItemName(id).ToString(), ItemRarity.GetColor(i.rare));
                            ImGui.EndTooltip();
                        }
                    }
                    if (count != 0)
                    {
                        string s = count.ToString();
                        ImFontPtr font = Client.ClientAssets.GetMonospaceFont(20);
                        System.Numerics.Vector2 pos = min + (max - min) / 2;
                        ImGui.PushFont(font);
                        pos.X -= ImGui.CalcTextSize(s).X / 2;

                        // for shadow 
                        pos.X -= 1;
                        drawList.AddText(font, font.FontSize, pos, 0xff000000, s);

                        pos.X += 2;
                        drawList.AddText(font, font.FontSize, pos, 0xff000000, s);

                        pos.X -= 1;
                        pos.Y -= 1;
                        drawList.AddText(font, font.FontSize, pos, 0xff000000, s);

                        pos.Y += 2f;
                        drawList.AddText(font, font.FontSize, pos, 0xff000000, s);

                        pos = min + (max - min) / 2;
                        pos.X -= ImGui.CalcTextSize(s).X / 2;
                        drawList.AddText(font, font.FontSize, pos, 0xffffffff, s);
                        ImGui.PopFont();
                    }
                }
            }
            buttonMin = min.ToXNA();
            buttonMax = max.ToXNA();
            return clicked;
        }

        public static void DrawItemCentered(ImDrawListPtr drawList, int id, Vector2 center, float size, int count = 0, float countFontSize = 20f)
        {
            Vector2 v = new Vector2(size);
            DrawItem(drawList, id, center - v / 2f, v, count, countFontSize);
        }

        public static void DrawItem(ImDrawListPtr drawList, int id, Vector2 position, Vector2 size, int count = 0, float countFontSize = 20f)
        {
            if (ItemImages[id] == IntPtr.Zero)
            {
                ItemImages[id] = (IntPtr)(-1);
                lock (ItemsToLoad)
                {
                    ItemsToLoad[itemPtr] = id;
                    itemPtr++;
                }
            }
            if (ItemImages[id] != (IntPtr)(-1))
            {
                Texture2D value = TextureAssets.Item[id].Value;
                if (TextureAssets.Item[id].IsLoaded)
                {
                    bool animated = Main.itemAnimations[id] != null;

                    if (!animated)
                    {
                        int width = value.Width;
                        int height = value.Height;
                        float num = 1f;
                        if ((float)width > size.X || (float)height > size.Y)
                        {
                            num = ((width <= height) ? (size.X / (float)height) : (size.Y / (float)width));
                        }
                        Vector2 scaledEnd = (new Vector2(width, height) * num);
                        drawList.AddImage(ItemImages[id], position.ToNumerics(), (position + scaledEnd).ToNumerics());
                    }
                    else
                    {
                        Rectangle animationRect = Main.itemAnimations[id].GetFrame(value);

                        int width = animationRect.Width;
                        int height = animationRect.Height;
                        float num = 1f;
                        if ((float)width > size.X || (float)height > size.Y)
                        {
                            num = ((width <= height) ? (size.X / (float)height) : (size.Y / (float)width));
                        }
                        Vector2 scaledEnd = (new Vector2(width, height) * num);
                        drawList.AddImage(ItemImages[id], position.ToNumerics(), (position + scaledEnd).ToNumerics());

                        System.Numerics.Vector2 rectStart = new System.Numerics.Vector2(animationRect.X, animationRect.Y);
                        System.Numerics.Vector2 rectEnd = new System.Numerics.Vector2(animationRect.X + width, animationRect.Y + height);

                        System.Numerics.Vector2 uvMin = rectStart / new System.Numerics.Vector2(value.Width, value.Height);
                        System.Numerics.Vector2 uvMax = rectEnd / new System.Numerics.Vector2(value.Width, value.Height);
                        drawList.AddImage(ItemImages[id], position.ToNumerics(), (position + scaledEnd).ToNumerics(), uvMin, uvMax);

                    }

                    if (count != 0)
                    {
                        string s = count.ToString();
                        ImFontPtr font = Client.ClientAssets.GetMonospaceFont(countFontSize);
                        Vector2 pos = position + size / 2;
                        ImGui.PushFont(font);
                        pos.X -= ImGui.CalcTextSize(s).X / 2;

                        // for shadow 
                        pos.X -= 1;
                        drawList.AddText(font, font.FontSize, pos.ToNumerics(), 0xff000000, s);

                        pos.X += 2;
                        drawList.AddText(font, font.FontSize, pos.ToNumerics(), 0xff000000, s);

                        pos.X -= 1;
                        pos.Y -= 1;
                        drawList.AddText(font, font.FontSize, pos.ToNumerics(), 0xff000000, s);

                        pos.Y += 2f;
                        drawList.AddText(font, font.FontSize, pos.ToNumerics(), 0xff000000, s);

                        pos = position + size / 2;
                        pos.X -= ImGui.CalcTextSize(s).X / 2;
                        drawList.AddText(font, font.FontSize, pos.ToNumerics(), 0xffffffff, s);
                        ImGui.PopFont();
                    }
                }
            }
        }

        public static bool WrappedSelectable(string text, float wrapWidth)
        {
            NVector2 textSize = ImGui.CalcTextSize(text, wrapWidth);

            ImGui.PushID(text);
            bool v = ImGui.Selectable("", false, ImGuiSelectableFlags.None, textSize);
            ImGui.PopID();

            ImGui.SetCursorScreenPos(ImGui.GetItemRectMin() + ImGui.GetStyle().ItemSpacing);

            ImGui.PushTextWrapPos(wrapWidth);
            ImGui.TextWrapped(text);
            ImGui.PopTextWrapPos();

            return v;
        }
    }
}
