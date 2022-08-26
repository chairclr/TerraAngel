namespace TerraAngel.Cheat
{
    public abstract class Cringe
    {
        /// <summary>
        /// Name used for sorting
        /// </summary>
        public virtual string Name => GetType().Name;

        /// <summary>
        /// Tab to display the cringe in
        /// </summary>
        public virtual CringeTabs Tab => CringeTabs.None;

        public abstract void DrawUI(ImGuiIOPtr io);

        public virtual void Update()
        {

        }
    }
}
