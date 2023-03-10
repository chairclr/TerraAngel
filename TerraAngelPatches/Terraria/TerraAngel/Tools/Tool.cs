namespace TerraAngel.Tools;

public abstract class Tool
{
    /// <summary>
    /// Name used for sorting
    /// </summary>
    public virtual string Name => GetType().Name;

    /// <summary>
    /// Tab to display the tool in
    /// </summary>
    public virtual ToolTabs Tab => ToolTabs.None;

    public virtual void DrawUI(ImGuiIOPtr io)
    {

    }

    public virtual void Update()
    {

    }
}
