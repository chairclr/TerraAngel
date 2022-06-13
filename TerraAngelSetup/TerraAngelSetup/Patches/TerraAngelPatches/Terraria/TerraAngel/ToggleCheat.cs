using ImGuiNET;

namespace TerraAngel
{
    public class ToggleCheat
    {
        public bool IsEnabled { get; set; }
        public virtual string Name { get; }

        public virtual void Load()
        {

        }

        public virtual void PreDraw()
        {

        }
        public virtual void PostDraw()
        {

        }

        public virtual void Enable()
        {

        }
        public virtual void Disable()
        {

        }
    }
}