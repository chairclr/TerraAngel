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
        private bool lastEnabled;
        public bool Enabled;
        public virtual string Name => GetType().Name;
        public virtual CringeTabs Tab => CringeTabs.None;

        public virtual void DrawUI(ImGuiIOPtr io)
        {
            ImGui.Checkbox(Name, ref Enabled);
        }

        public virtual void Update()
        {
            if (lastEnabled != Enabled)
            {
                if (Enabled)
                    OnEnable();
                else
                    OnDisable();
            }
            lastEnabled = Enabled;
        }

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
    }
}
