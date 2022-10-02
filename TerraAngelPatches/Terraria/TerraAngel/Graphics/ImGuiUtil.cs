using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Chat;
using Terraria.UI.Chat;

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
        public static void TextColored(string text, uint color)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextUnformatted(text);
            ImGui.PopStyleColor();
        }
        public static void ColorEdit3(string label, ref Color color)
        {
            System.Numerics.Vector3 v3c = color.ToVector3().ToNumerics();
            if (ImGui.ColorEdit3(label, ref v3c))
            {
                color = new Color(v3c.X, v3c.Y, v3c.Z, (color.A / 255f));
            }
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

        private static IntPtr[] ItemImages = new IntPtr[ItemID.Count];
        public static bool ItemButton(int id, string uid, bool showTooltip = true, bool isSelected = false, float margin = 6f, float alpha = 1.0f)
        {
            return ItemButton(ContentSamples.ItemsByType[id], uid, new Vector2(32, 32), showTooltip, isSelected, margin, 18f, alpha);
        }
        public static bool ItemButton(int id, string uid, Vector2 size, bool showTooltip = true, bool isSelected = false, float margin = 6f, float alpha = 1.0f)
        {
            return ItemButton(ContentSamples.ItemsByType[id], uid, size, showTooltip, isSelected, margin, 18f, alpha);
        }
        public static bool ItemButton(Item item, string uid, Vector2 size, bool showTooltip = true, bool isSelected = false, float margin = 6f, float countFontSize = 18f, float alpha = 1.0f)
        {
            int id = item.type;
            NVector2 drawSize = size.ToNumerics();
            ImGui.PushID(uid);
            bool clicked = false;
            if (ImGui.Button("", drawSize + new NVector2(margin)))
            {
                clicked = true;
            }
            NVector2 min = ImGui.GetItemRectMin();
            NVector2 max = ImGui.GetItemRectMax();
            ImGui.PopID();

            if (ImGui.IsRectVisible(min, max))
            {
                ImDrawListPtr drawList = ImGui.GetWindowDrawList();
                if (isSelected)
                {
                    drawList.AddRectFilledMultiColor(min, max, 0xFF1de5ff, 0xFFa4ffff, 0xFF1de5ff, 0xFF00b4ff);
                }

                drawList.DrawItem(item, (min).ToXNA() + new Vector2(margin) / 2f, size, countFontSize, alpha);

                if (showTooltip && id != ItemID.None)
                {
                    if (ImGui.IsMouseHoveringRect(min, max))
                    {
                        // relogic code?
                        try
                        {
                            ImGuiItemTooltip(item);
                        }
                        catch
                        {

                        }
                    }
                }
            }
            return clicked;
        }

        public static void DrawRay(this ImDrawListPtr drawList, RaycastData data, uint col)
        {
            drawList.AddLine(Util.WorldToScreen(data.Origin).ToNumerics(), Util.WorldToScreen(data.End).ToNumerics(), col);
        }
        public static void DrawItemCentered(ImDrawListPtr drawList, int id, Vector2 center, float size, float countFontSize = 14f, float alpha = 1.0f)
        {
            Vector2 v = new Vector2(size);
            DrawItem(drawList, id, center - v / 2f, v, countFontSize, alpha);
        }

        public static void DrawItemCentered(ImDrawListPtr drawList, Item item, Vector2 center, float size, float countFontSize = 14f, float alpha = 1.0f)
        {
            Vector2 v = new Vector2(size);
            DrawItem(drawList, item, center - v / 2f, v, countFontSize, alpha);
        }

        public static void DrawItem(this ImDrawListPtr drawList, int id, Vector2 position, Vector2 size, float countFontSize = 14f, float alpah = 1.0f)
        {
            DrawItem(drawList, ContentSamples.ItemsByType[id], position, size, countFontSize, alpah);
        }

        public static void DrawItem(this ImDrawListPtr drawList, Item item, Vector2 position, Vector2 size, float countFontSize = 14f, float alpah = 1.0f)
        {
            int id = item.type;
            if (ItemImages[id] == IntPtr.Zero)
            {
                Main.instance.LoadItem(id);
                ItemImages[id] = ClientLoader.MainRenderer?.BindTexture(TextureAssets.Item[id].Value) ?? IntPtr.Zero;
            }

            

            if (ItemImages[id] != (IntPtr)(-1))
            {
                Texture2D value = TextureAssets.Item[id].Value;
                if (TextureAssets.Item[id].IsLoaded)
                {
                    bool animated = Main.itemAnimations[id] != null;

                    if (animated)
                    {
                        Rectangle animationRect = Main.itemAnimations[id].GetFrame(value);

                        int width = animationRect.Width;
                        int height = animationRect.Height;
                        float num = 1f;
                        if (width > size.X || height > size.Y)
                        {
                            num = ((width <= height) ? (size.X / height) : (size.Y / width));
                        }
                        NVector2 rectStart = new NVector2(animationRect.X, animationRect.Y);
                        NVector2 rectEnd = new NVector2(animationRect.X + width, animationRect.Y + height);
                        NVector2 uvMin = rectStart / new NVector2(value.Width, value.Height);
                        NVector2 uvMax = rectEnd / new NVector2(value.Width, value.Height);


                        Vector2 scaledSize = (new Vector2(width, height) * num);
                        Vector2 start = position + (size / 2f) - scaledSize / 2f;

                        drawList.AddImage(ItemImages[id], start.Floor().ToNumerics(), (start + scaledSize).Floor().ToNumerics(), uvMin, uvMax, Color.White.WithAlpha(alpah).PackedValue);
                    }
                    else
                    {
                        int width = value.Width;
                        int height = value.Height;
                        float num = 1f;
                        if (width > size.X || height > size.Y)
                        {
                            num = ((width <= height) ? (size.X / height) : (size.Y / width));
                        }
                        Vector2 scaledSize = (new Vector2(width, height) * num);
                        Vector2 start = position + (size / 2f) - scaledSize / 2f;
                        drawList.AddImage(ItemImages[id], start.Floor().ToNumerics(), (start + scaledSize).Floor().ToNumerics(), NVector2.Zero, NVector2.One, Color.White.WithAlpha(alpah).PackedValue);
                    }

                    if (item.stack > 1)
                    {
                        string s = item.stack.ToString();
                        ImFontPtr font = Client.ClientAssets.GetTerrariaFont(countFontSize);
                        ImGui.PushFont(font);
                        Vector2 pos = ((position + size / 2f) - new Vector2(ImGui.CalcTextSize(s).X / 2f, 0f));

                        // for shadow 
                        pos.X -= 1;
                        drawList.AddText(font, font.FontSize, pos.ToNumerics(), Color.Black.WithAlpha(alpah).PackedValue, s);

                        pos.X += 2;
                        drawList.AddText(font, font.FontSize, pos.ToNumerics(), Color.Black.WithAlpha(alpah).PackedValue, s);

                        pos.X -= 1;
                        pos.Y -= 1;
                        drawList.AddText(font, font.FontSize, pos.ToNumerics(), Color.Black.WithAlpha(alpah).PackedValue, s);

                        pos.Y += 2f;
                        drawList.AddText(font, font.FontSize, pos.ToNumerics(), Color.Black.WithAlpha(alpah).PackedValue, s);

                        pos = ((position + size / 2f) - new Vector2(ImGui.CalcTextSize(s).X / 2f, 0f));
                        drawList.AddText(font, font.FontSize, pos.ToNumerics(), Color.White.WithAlpha(alpah).PackedValue, s);
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

        public static bool WrappedSelectable(string id, string text, float wrapWidth)
        {
            NVector2 textSize = ImGui.CalcTextSize(text, wrapWidth);

            ImGui.PushID(id);
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

        public unsafe static NVector2 CalcTextSizeWithTags(List<TextSnippet> tags, float wrapWidth)
        {
            ImFontPtr font = ImGui.GetFont();
            float itemTagSpacingWidth = ImGui.CalcTextSize(" ").X / 2f;

            NVector2 textSize = new NVector2(0f, font.FontSize);
            NVector2 offset = textSize;

            bool renderedWord = false;

            float spaceLeft = wrapWidth;
            for (int i = 0; i < tags.Count; i++)
            {

                if (tags[i] is ItemTagHandler.ItemSnippet)
                {
                    float width = itemTagSpacingWidth * 2f + font.FontSize;

                    if (renderedWord && width > spaceLeft)
                    {
                        spaceLeft = wrapWidth - width;

                        offset.Y += font.FontSize;
                        offset.X = 0;
                        offset.X = width;
                        textSize.X = MathF.Max(textSize.X, offset.X);
                        textSize.Y = MathF.Max(textSize.Y, offset.Y);
                    }
                    else
                    {
                        spaceLeft -= width;
                        offset.X += width;
                        textSize.X = MathF.Max(textSize.X, offset.X);
                        textSize.Y = MathF.Max(textSize.Y, offset.Y);
                        renderedWord = true;
                    }
                }
                else
                {
                    string text = tags[i].Text;

                    string[] words = Regex.Split(text, @"(?<=[.,;\s])");

                    for (int j = 0; j < words.Length; j++)
                    {
                        if (string.IsNullOrEmpty(words[j]))
                            continue;


                        float wordWidth = ImGui.CalcTextSize(words[j]).X;

                        if (words[j] == "\n" && wordWidth == 0)
                        {
                            spaceLeft = wrapWidth - wordWidth;

                            offset.Y += font.FontSize;
                            offset.X = 0;
                            offset.X = wordWidth;
                            textSize.X = MathF.Max(textSize.X, offset.X);
                            textSize.Y = MathF.Max(textSize.Y, offset.Y);
                        }

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
            }

            return textSize;
        }

        public static void ImGuiItemTooltip(Item item)
        {
            ImGui.BeginTooltip();

            int yoyoLogo = 0;
            int researchLine = 0;
            int numLines = 1;
            string[] array = new string[30];
            bool[] goodPrefixLine = new bool[30];
            bool[] badPrefixLine = new bool[30];

            Main.MouseText_DrawItemTooltip_GetLinesInfo(item, ref yoyoLogo, ref researchLine, item.knockBack, ref numLines, array, goodPrefixLine, badPrefixLine);

            for (int i = 0; i < numLines; i++)
            {
                Color color = Color.White;
                if (i == 0) color = ItemRarity.GetColor(item.rare);
                if (goodPrefixLine[i]) color = new Color(117, 185, 117);
                if (badPrefixLine[i]) color = new Color(185, 117, 117);
                TextColored(ChatManager.ParseMessage(array[i], color).StringSum(x => x.Text), color);
            }

            ImGui.EndTooltip();
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

                    if (words[i] == "\n" && wordSize.X == 0)
                    {
                        spaceLeft = wrapWidth - wordSize.X;

                        offset.Y += font.FontSize;
                        offset.X = 0;
                        offset.X = wordSize.X;
                        textSize.X = MathF.Max(textSize.X, offset.X);
                        textSize.Y = MathF.Max(textSize.Y, offset.Y);
                    }

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
                                ImGuiItemTooltip(snippet._item);
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
            float itemTagSpacingWidth = ImGui.CalcTextSize(" ").X / 2f;

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            NVector2 offset = NVector2.Zero;

            bool renderedWord = false;

            float spaceLeft = wrapWidth;
            for (int i = 0; i < tags.Count; i++)
            {
                if (tags[i] is ItemTagHandler.ItemSnippet)
                {
                    ItemTagHandler.ItemSnippet snippet = (ItemTagHandler.ItemSnippet)tags[i];
                    NVector2 wordSize = new NVector2(font.FontSize);

                    if (renderedWord && (wordSize.X + itemTagSpacingWidth * 2f) > spaceLeft)
                    {
                        spaceLeft = wrapWidth - (wordSize.X + itemTagSpacingWidth * 2f);

                        offset.Y += font.FontSize;
                        offset.X = itemTagSpacingWidth;

                        DrawItemCentered(drawList, snippet._item, ((min + offset + min + offset + wordSize) / 2f).ToXNA(), font.FontSize, 18f, alpha);

                        if (ImGui.IsMouseHoveringRect(min + offset, min + offset + wordSize))
                        {
                            ImGuiItemTooltip(snippet._item);
                            ImGui.GetIO().WantCaptureMouse = true;
                        }

                        offset.X += wordSize.X + itemTagSpacingWidth;
                    }
                    else
                    {
                        spaceLeft -= (wordSize.X + itemTagSpacingWidth * 2f);

                        offset.X += itemTagSpacingWidth;

                        DrawItemCentered(drawList, snippet._item, ((min + offset + min + offset + wordSize) / 2f).ToXNA(), font.FontSize, 18f, alpha);

                        if (ImGui.IsMouseHoveringRect(min + offset, min + offset + wordSize))
                        {
                            ImGuiItemTooltip(snippet._item);
                            ImGui.GetIO().WantCaptureMouse = true;
                        }
                        offset.X += wordSize.X + itemTagSpacingWidth;
                        renderedWord = true;
                    }

                }
                else
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

                        if (words[j] == "\n" && wordSize.X == 0)
                        {
                            spaceLeft = wrapWidth - wordSize.X;

                            offset.Y += font.FontSize;
                            offset.X = 0;
                            offset.X = wordSize.X;
                            textSize.X = MathF.Max(textSize.X, offset.X);
                            textSize.Y = MathF.Max(textSize.Y, offset.Y);
                        }

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
                            offset.X += wordSize.X;
                            renderedWord = true;
                        }
                    }
                }
            }

            return v;
        }
    }
}
