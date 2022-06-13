using ImGuiNET;

namespace TerraAngel
{
    public abstract class ToggleCheat
    {
        public bool IsEnabled { get; set; }
        public virtual string Name => GetType().Name;
        public virtual string Description => "";

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