using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using Terraria;
namespace TerraAngel
{
    public class ClientLoader
    {
        static bool lateInit = false;

        public static void Test_Setup_Early(Game main)
        {
        }

        public static void Test_Setup_Late(Game main)
        {
            TerraAngel.Graphics.TerraRenderer.Init(main);
            lateInit = true;
        }

        public static void Test_DrawUI(GameTime time)
        {
            if (lateInit)
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                TerraAngel.Graphics.TerraRenderer.Draw(time);
                Main.spriteBatch.End();
            }
        }

        public static void Test_Draw_Real()
        {
            ImGui.Begin("Test window");

            if (ImGui.Button("Close the game"))
            {
                Environment.Exit(-1);
            }

            ImGui.End();
        }
    }
}
