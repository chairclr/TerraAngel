namespace TerraAngel;

public class Sexer
{
    public static readonly string DefaultSexerName = "DefaultSexer";

    public readonly string SexerName;

    public static Sexer CreateDefaultSexer()
    {
        return new Sexer();
    }

    public Sexer()
    {
        SexerName = DefaultSexerName;
    }

    public Sexer(string name)
    {
        SexerName = name;
    }
}