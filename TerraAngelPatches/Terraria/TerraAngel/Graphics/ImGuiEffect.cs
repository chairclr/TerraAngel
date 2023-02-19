using System.IO;

namespace TerraAngel.Graphics;

public class ImGuiEffect : Effect
{
    private EffectParameter TextureParam;

    private EffectParameter WorldViewProjParam;

    public Matrix World = Matrix.Identity;

    public Matrix View = Matrix.Identity;

    public Matrix Projection = Matrix.Identity;

    public Texture2D Texture
    {
        get => TextureParam.GetValueTexture2D();
        set => TextureParam.SetValue(value);
    }

    public ImGuiEffect(GraphicsDevice graphicsDevice, string path)
        : base(graphicsDevice, File.ReadAllBytes(path))
    {
        TextureParam = Parameters["Texture"];
        WorldViewProjParam = Parameters["WorldViewProj"];
    }

    protected override void OnApply()
    {
        WorldViewProjParam.SetValue(World * View * Projection);

        base.OnApply();
    }
}
