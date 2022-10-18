namespace TerraAngel.Graphics;

public class GraphicsUtility
{
    public static Texture2D BlankTexture;
    public static Texture2D MissingTexture;

    static GraphicsUtility()
    {
        BlankTexture = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
        BlankTexture.SetData(new[] { Color.White });

        MissingTexture = new Texture2D(Main.graphics.GraphicsDevice, 2, 2);
        MissingTexture.SetData(new[] { new Color(0xe2, 0x32, 0xe2), Color.Black,
                                       Color.Black, new Color(0xe2, 0x32, 0xe2) });
    }
}
