using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Terraria.Audio;

namespace TerraAngel.Tools.Builder;

public class BuilderModeTool : Tool
{
    public override string Name => "Builder Tool Toggle";

    public override ToolTabs Tab => ToolTabs.None;

    public bool Enabled = false;

    public override void Update()
    {
        if (InputSystem.Ctrl && InputSystem.IsKeyPressed(Keys.B))
        {
            Enabled = !Enabled;

            if (Enabled)
                ClientLoader.Console.WriteLine("Builder mode activated");
            else
                ClientLoader.Console.WriteLine("Builder mode deactivated");
        }
    }
}
