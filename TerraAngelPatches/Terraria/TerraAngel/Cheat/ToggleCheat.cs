using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace TerraAngel.Cheat
{
    public abstract class Cringe
    {
        public bool Enabled;
        public virtual string Name => GetType().Name;
        public virtual CringeTabs Tab => CringeTabs.None;

        public virtual void DrawUI(ImGuiIOPtr io)
        {
            ImGui.Checkbox(Name, ref Enabled);
        }
    }
}
