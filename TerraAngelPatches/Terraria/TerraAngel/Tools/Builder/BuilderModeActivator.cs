using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace TerraAngel.Tools.Builder;

public class BuilderModeActivator : Tool
{
    public override void Update()
    {
        if (InputSystem.Ctrl && InputSystem.IsKeyPressed(Keys.B))
        {
            BuilderModeTool.Enabled = !BuilderModeTool.Enabled;
        }
    }
}
