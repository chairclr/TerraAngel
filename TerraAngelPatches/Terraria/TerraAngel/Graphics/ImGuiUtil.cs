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
using Terraria.UI.Chat;
using System.Text.RegularExpressions;
using Terraria.GameContent.UI.Chat;

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

            NVector2 min = ImGui.GetItemRectMin();
            NVector2 spacing = ImGui.GetStyle().ItemSpacing;

            NVector2 pos = min + spacing;

            ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();

            windowDrawList.AddText(ImGui.GetFont(), ImGui.GetFontSize(), pos, text, ImGui.GetColorU32(ImGuiCol.Text), wrapWidth);

            return v;
        }

        public static unsafe void AddText(this ImDrawListPtr drawList, ImFontPtr font, float fontSize, NVector2 pos, string text, uint color, float wrapWidth)
        {
            int textByteCount = Encoding.UTF8.GetByteCount(text);
            byte* nativeTextPtr = stackalloc byte[textByteCount + 1];
            fixed (char* textStartPtr = text)
            {
                int native_text_begin_offset = Encoding.UTF8.GetBytes(textStartPtr, text.Length, nativeTextPtr, textByteCount);
                nativeTextPtr[native_text_begin_offset] = 0;
            }
            byte* native_text_end = null;

            ImGuiNative.ImDrawList_AddText_FontPtr(drawList.NativePtr, font.NativePtr, fontSize, pos, color, nativeTextPtr, native_text_end, wrapWidth, null);
        }

        public static unsafe void AddText(this ImDrawListPtr drawList, NVector2 pos, string text, uint color, float wrapWidth)
        {
            int textByteCount = Encoding.UTF8.GetByteCount(text);
            byte* nativeTextPtr = stackalloc byte[textByteCount + 1];
            fixed (char* textStartPtr = text)
            {
                int native_text_begin_offset = Encoding.UTF8.GetBytes(textStartPtr, text.Length, nativeTextPtr, textByteCount);
                nativeTextPtr[native_text_begin_offset] = 0;
            }
            byte* native_text_end = null;

            ImGuiNative.ImDrawList_AddText_FontPtr(drawList.NativePtr, ImGui.GetFont().NativePtr, ImGui.GetFontSize(), pos, color, nativeTextPtr, native_text_end, wrapWidth, null);
        }

        static char[] lengths = (new int[]{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 3, 3, 4, 0 }).Select(x => (char)x).ToArray();
        static int[] masks = { 0x00, 0x7f, 0x1f, 0x0f, 0x07 };
        static uint[] mins = { 0x400000, 0, 0x80, 0x800, 0x10000 };
        static int[] shiftc = { 0, 18, 12, 6, 0 };
        static int[] shifte = { 0, 6, 4, 2, 0 };
        public unsafe static NVector2 CalcTextSizeWithTags(List<TextSnippet> tags, float wrapWidth)
        {
            ImFontPtr font = ImGui.GetFont();

            NVector2 textSize = new NVector2(0f, font.FontSize);
            NVector2 offset = textSize;

            bool renderedWord = false;

            float spaceLeft = wrapWidth;
            for (int i = 0; i < tags.Count; i++)
            {
                string text = tags[i].Text;

                string[] words = Regex.Split(text, @"(?<=[.,;\s])");

                for (int j = 0; j < words.Length; j++)
                {
                    if (string.IsNullOrEmpty(words[j]))
                        continue;

                    float wordWidth = ImGui.CalcTextSize(words[j]).X;

                    if (wordWidth == 0)
                        continue;

                    if (renderedWord && wordWidth > spaceLeft)
                    {
                        spaceLeft = wrapWidth - wordWidth;

                        offset.Y += font.FontSize;
                        offset.X = 0;
                        offset.X = wordWidth;
                        textSize.X = MathF.Max(textSize.X, offset.X);
                        textSize.Y = MathF.Max(textSize.Y, offset.Y);
                    }
                    else
                    {
                        spaceLeft -= wordWidth;
                        offset.X += wordWidth;
                        textSize.X = MathF.Max(textSize.X, offset.X);
                        textSize.Y = MathF.Max(textSize.Y, offset.Y);
                        renderedWord = true;
                    }
                }
            }

            return textSize;
        }

        public static bool WrappedSelectableWithTextBorder(string text, float wrapWidth, Color borderColor)
        {
            NVector2 textSize = ImGui.CalcTextSize(text, wrapWidth);

            ImGui.PushID(text);
            bool v = ImGui.Selectable("", false, ImGuiSelectableFlags.None, textSize);
            ImGui.PopID();

            NVector2 min = ImGui.GetItemRectMin();
            NVector2 spacing = ImGui.GetStyle().ItemSpacing;

            NVector2 pos = min + spacing;

            ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();


            windowDrawList.AddText(pos - NVector2.UnitX, text, borderColor.PackedValue, wrapWidth);
            windowDrawList.AddText(pos + NVector2.UnitX, text, borderColor.PackedValue, wrapWidth);
            windowDrawList.AddText(pos - NVector2.UnitY, text, borderColor.PackedValue, wrapWidth);
            windowDrawList.AddText(pos + NVector2.UnitY, text, borderColor.PackedValue, wrapWidth);

            windowDrawList.AddText(pos, text, ImGui.GetColorU32(ImGuiCol.Text), wrapWidth);
            return v;
        }
        public static bool WrappedSelectableWithTextBorderWithTags(string id, List<TextSnippet> tags, float wrapWidth, Color borderColor, float alpha = 1.0f)
        {
            borderColor.A = (byte)(alpha * 255f);
            NVector2 textSize = CalcTextSizeWithTags(tags, wrapWidth);

            ImGui.PushID(id);
            bool v = ImGui.Selectable("", false, ImGuiSelectableFlags.None, textSize);
            ImGui.PopID();

            NVector2 min = ImGui.GetItemRectMin() + ImGui.GetStyle().ItemSpacing;

            ImFontPtr font = ImGui.GetFont();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            NVector2 offset = NVector2.Zero;

            bool renderedWord = false;

            float spaceLeft = wrapWidth;
            for (int i = 0; i < tags.Count; i++)
            {
                string text = tags[i].Text;
                Color tagColor = tags[i].Color;

                tagColor.A = (byte)(alpha * 255f);

                string[] words = Regex.Split(text, @"(?<=[.,;\s])");

                for (int j = 0; j < words.Length; j++)
                {
                    if (string.IsNullOrEmpty(words[j]))
                        continue;

                    NVector2 wordSize = ImGui.CalcTextSize(words[j]);

                    if (wordSize.X == 0)
                        continue;

                    if (renderedWord && wordSize.X > spaceLeft)
                    {
                        spaceLeft = wrapWidth - wordSize.X;

                        offset.Y += font.FontSize;
                        offset.X = 0;
                        drawList.AddText(min + offset - NVector2.UnitX * 2f, borderColor.PackedValue, words[j]);
                        drawList.AddText(min + offset + NVector2.UnitX * 2f, borderColor.PackedValue, words[j]);
                        drawList.AddText(min + offset - NVector2.UnitY * 2f, borderColor.PackedValue, words[j]);
                        drawList.AddText(min + offset + NVector2.UnitY * 2f, borderColor.PackedValue, words[j]);
                        drawList.AddText(min + offset, tagColor.PackedValue, words[j]);
                        offset.X = wordSize.X;
                    }
                    else
                    {
                        spaceLeft -= wordSize.X;
                        drawList.AddText(min + offset - NVector2.UnitX * 2f, borderColor.PackedValue, words[j]);
                        drawList.AddText(min + offset + NVector2.UnitX * 2f, borderColor.PackedValue, words[j]);
                        drawList.AddText(min + offset - NVector2.UnitY * 2f, borderColor.PackedValue, words[j]);
                        drawList.AddText(min + offset + NVector2.UnitY * 2f, borderColor.PackedValue, words[j]);
                        drawList.AddText(min + offset, tagColor.PackedValue, words[j]);

                        if (tags[i] is ItemTagHandler.ItemSnippet)
                        {
                            ItemTagHandler.ItemSnippet snippet = (ItemTagHandler.ItemSnippet)tags[i];
                            if (ImGui.IsMouseHoveringRect(min + offset, min + offset + wordSize))
                            {
                                ImGui.BeginTooltip();

                                int yoyoLogo = 0;
                                int researchLine = 0;
                                int numLines = 1;
                                string[] array = new string[30];
                                bool[] goodPrefixLine = new bool[30];
                                bool[] badPrefixLine = new bool[30];

                                Main.MouseText_DrawItemTooltip_GetLinesInfo(snippet._item, ref yoyoLogo, ref researchLine, snippet._item.knockBack, ref numLines, array, goodPrefixLine, badPrefixLine);

                                for (int k = 0; k < numLines; k++)
                                {
                                    Color color = Color.White;
                                    if (k == 0) color = snippet.Color;
                                    if (goodPrefixLine[k]) color = new Color(117, 185, 117);
                                    if (badPrefixLine[k]) color = new Color(185, 117, 117);
                                    TextColored(array[k], color);
                                }


                                ImGui.EndTooltip();

                                ImGui.GetIO().WantCaptureMouse = true;
                            }
                        }

                        offset.X += wordSize.X;
                        renderedWord = true;
                    }
                }
            }

            return v;
        }
        public static bool WrappedSelectableWithTextBorderWithTags(string id, List<TextSnippet> tags, float wrapWidth, Color borderColor, NVector2 textSize, float alpha = 1.0f)
        {
            borderColor.A = (byte)(alpha * 255f);
            ImGui.PushID(id);
            bool v = ImGui.Selectable("", false, ImGuiSelectableFlags.None, textSize);
            ImGui.PopID();

            NVector2 min = ImGui.GetItemRectMin() + ImGui.GetStyle().ItemSpacing;

            ImFontPtr font = ImGui.GetFont();

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            NVector2 offset = NVector2.Zero;

            bool renderedWord = false;

            float spaceLeft = wrapWidth;
            for (int i = 0; i < tags.Count; i++)
            {
                string text = tags[i].Text;
                Color tagColor = tags[i].Color;

                tagColor.A = (byte)(alpha * 255f);

                string[] words = Regex.Split(text, @"(?<=[.,;\s])");

                for (int j = 0; j < words.Length; j++)
                {
                    if (string.IsNullOrEmpty(words[j]))
                        continue;

                    NVector2 wordSize = ImGui.CalcTextSize(words[j]);

                    if (wordSize.X == 0)
                        continue;

                    if (renderedWord && wordSize.X > spaceLeft)
                    {
                        spaceLeft = wrapWidth - wordSize.X;

                        offset.Y += font.FontSize;
                        offset.X = 0;
                        drawList.AddText(min + offset - NVector2.UnitX * 2f, borderColor.PackedValue, words[j]);
                        drawList.AddText(min + offset + NVector2.UnitX * 2f, borderColor.PackedValue, words[j]);
                        drawList.AddText(min + offset - NVector2.UnitY * 2f, borderColor.PackedValue, words[j]);
                        drawList.AddText(min + offset + NVector2.UnitY * 2f, borderColor.PackedValue, words[j]);
                        drawList.AddText(min + offset, tagColor.PackedValue, words[j]);
                        offset.X = wordSize.X;
                    }
                    else
                    {
                        spaceLeft -= wordSize.X;
                        drawList.AddText(min + offset - NVector2.UnitX * 2f, borderColor.PackedValue, words[j]);
                        drawList.AddText(min + offset + NVector2.UnitX * 2f, borderColor.PackedValue, words[j]);
                        drawList.AddText(min + offset - NVector2.UnitY * 2f, borderColor.PackedValue, words[j]);
                        drawList.AddText(min + offset + NVector2.UnitY * 2f, borderColor.PackedValue, words[j]);
                        drawList.AddText(min + offset, tagColor.PackedValue, words[j]);

                        if (tags[i] is ItemTagHandler.ItemSnippet)
                        {
                            ItemTagHandler.ItemSnippet snippet = (ItemTagHandler.ItemSnippet)tags[i];
                            if (ImGui.IsMouseHoveringRect(min + offset, min + offset + wordSize))
                            {
                                ImGui.BeginTooltip();

                                int yoyoLogo = 0;
                                int researchLine = 0;
                                int numLines = 1;
                                string[] array = new string[30];
                                bool[] goodPrefixLine = new bool[30];
                                bool[] badPrefixLine = new bool[30];

                                Main.MouseText_DrawItemTooltip_GetLinesInfo(snippet._item, ref yoyoLogo, ref researchLine, snippet._item.knockBack, ref numLines, array, goodPrefixLine, badPrefixLine);

                                for (int k = 0; k < numLines; k++)
                                {
                                    Color color = Color.White;
                                    if (k == 0) color = snippet.Color;
                                    if (goodPrefixLine[k]) color = new Color(117, 185, 117);
                                    if (badPrefixLine[k]) color = new Color(185, 117, 117);
                                    TextColored(array[k], color);
                                }


                                ImGui.EndTooltip();
                                ImGui.GetIO().WantCaptureMouse = true;
                            }
                        }

                        offset.X += wordSize.X;
                        renderedWord = true;
                    }
                }
            }

            return v;
        }
    }
}
