using System;
using System.Linq;
using System.Text.RegularExpressions;
namespace TerraAngel.Utility;

public static class StringExtensions
{
    public static int CompareStringDist(this string s, string t)
    {
        if (string.IsNullOrEmpty(t))
        {
            throw new ArgumentNullException(t, "String Cannot Be Null Or Empty");
        }

        int n = s.Length;
        int m = t.Length;

        if (n == 0)
        {
            return m;
        }

        if (m == 0)
        {
            return n;
        }

        int[] p = new int[n + 1];
        int[] d = new int[n + 1];

        int i;
        int j;

        for (i = 0; i <= n; i++)
        {
            p[i] = i;
        }

        for (j = 1; j <= m; j++)
        {
            char tJ = t[j - 1];
            d[0] = j;

            for (i = 1; i <= n; i++)
            {
                int cost = s[i - 1] == tJ ? 0 : 1;

                d[i] = Math.Min(Math.Min(d[i - 1] + 1, p[i] + 1), p[i - 1] + cost);
            }

            int[] dPlaceholder = p; //placeholder to assist in swapping p and d
            p = d;
            d = dPlaceholder;

        }

        // our last action in the above loop was to switch d and p, so p now 
        // actually has the most recent cost counts
        return p[n];
    }

    public static string[] EnumFancyNames(Type type)
    {
        if (!type.IsEnum)
            throw new ArgumentException(nameof(type));

        return Enum.GetNames(type).Select((x) => x.ToSentenceCase()).ToArray();
    }

    public static string[] EnumFancyNames<TEnum>()
        where TEnum : Enum
    {
        return Enum.GetNames(typeof(TEnum)).Select((x) => x.ToSentenceCase()).ToArray();
    }

    public static string EscapeString(string value)
    {
        try
        {
            value = Regex.Unescape(value);
        }
        catch (RegexParseException)
        {

        }
        return value;
    }

    public static string ToSentenceCase(this string str)
    {
        return Regex.Replace(str, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
    }

    public static string Truncate(this string value, int maxChars)
    {
        return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
    }
}