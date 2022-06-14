using Microsoft.Xna.Framework;
using TerraAngel.Hooks;
using TerraAngel.Graphics;
using TerraAngel.Input;
using TerraAngel.Client;
using TerraAngel.Client.Config;

namespace TerraAngel.Loader
{
    public class ClientLoader
    {
        public static ClientRenderer MainRenderer;

        public static bool SetupRenderer = false;

        public static ConfigUI ConfigUI = new ConfigUI();

        public static void Hookgen_Early()
        {
            GameHooks.Generate();
        }

        public static void SetupImGuiRenderer(Game main)
        {
            if (!SetupRenderer)
            {
                SetupRenderer = true;
                MainRenderer = new ClientRenderer(main);
            }
        }
    }
}
