namespace TerraAngel.Plugin
{
    public interface IPlugin
    {
        public string Name { get; }

        public void Load();
        public void Unload();
    }
}