using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TerraAngel.Cheat
{
    public class CringeManager
    {
        private static Dictionary<Type, Cringe> cringes = new Dictionary<Type, Cringe>();
        private static List<Cringe>[] cringesAsTabs;

        static CringeManager()
        {
            cringesAsTabs = new List<Cringe>[Enum.GetValues<CringeTabs>().Length];
            for (int i = 0; i < cringesAsTabs.Length; i++)
            {
                cringesAsTabs[i] = new List<Cringe>();
            }
        }

        public static T GetCringe<T>() where T : Cringe
        {
            return (T)cringes[typeof(T)];
        }
        public static void AddCringe<T>(bool enabled = false) where T : Cringe
        {
            if (!cringes.ContainsKey(typeof(T)))
            {
                Cringe cringe = Activator.CreateInstance<T>();
                cringe.Enabled = enabled;
                cringesAsTabs[(int)cringe.Tab].Add(cringe);
                cringes.Add(typeof(T), cringe);
            }
        }
        public static void RemoveCringe<T>() where T : Cringe
        {
            if (cringes.ContainsKey(typeof(T)))
            {
                Cringe? cringe = GetCringe<T>();
                if (cringe is not null)
                    cringesAsTabs[(int)cringe.Tab].Remove(cringe);
                cringes.Remove(typeof(T));
            }
        }

        public static Cringe GetCringe(Type type)
        {
            return cringes[type];
        }
        public static void AddCringe(Type type, bool enabled = false)
        {
            if (!cringes.ContainsKey(type))
            {
                Cringe cringe = (Cringe)Activator.CreateInstance(type);
                cringe.Enabled = enabled;
                cringesAsTabs[(int)cringe.Tab].Add(cringe);
                cringes.Add(type, cringe);
            }
        }
        public static void RemoveCringe(Type type)
        {
            if (cringes.ContainsKey(type))
            {
                Cringe? cringe = GetCringe(type);
                if (cringe is not null)
                    cringesAsTabs[(int)cringe.Tab].Remove(cringe);
                cringes.Remove(type);
            }
        }

        public static IEnumerable<Cringe> GetCringeOfTab(CringeTabs tab)
        {
            return cringesAsTabs[(int)tab];
        }

        public static void SortTabs()
        {
            foreach (List<Cringe> tab in cringesAsTabs)
            {
                tab.Sort((x, y) => x.Name.CompareTo(y.Name));
            }
        }
        

        public static int ButcherDamage = 1000;

        public static bool AutoButcherHostileNPCs = false;

        public static bool NebulaSpam = false;
        public static int NebulaSpamPower = 100;

        public static bool[,] LoadedTileSections;
    }
}
