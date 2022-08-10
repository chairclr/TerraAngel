using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraAngel
{
    public class Sexer
    {
        public const string SexerName = "DefaultSexer";

        public static Sexer CreateSexer()
        {
            return new Sexer();
        }

        public Sexer()
        {

        }
    }
}