using System.Collections.Generic;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;

namespace TerraAngel.Client
{
    public abstract class ClientWindow
    {
        public virtual bool IsEnabled { get; set; }

        public virtual bool DefaultEnabled => true;

        public virtual bool IsToggleable => true;

        public virtual string Title => GetType().Name;

        public virtual Keys ToggleKey => Keys.None;

        public virtual void Init() { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        public abstract void Draw(ImGuiIOPtr io);
        public virtual void Update() { }
    }
}
