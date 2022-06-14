using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraAngel.Utility
{
    public class Util
    {
        private static string[] ByteSizeNames = { "b", "k", "m", "g", "t", "p" };

        public static string PrettyPrintBytes(long bytes, string format = "{0:F2}{1}")
        {
            float len = bytes;
            int order = 0;
            while ((len >= 1024 || len >= 100f) && order < ByteSizeNames.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return string.Format(format, len, ByteSizeNames[order]);
        }
    }
}
