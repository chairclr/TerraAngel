using System;
using System.Collections.Generic;

namespace TerraAngel.Utility;

public static class ListExtensions
{
    public static string StringSum<T>(this List<T> e, Func<T, string> predicate)
    {
        string s = "";
        e.ForEach((x) => s += predicate(x));
        return s;
    }
}
