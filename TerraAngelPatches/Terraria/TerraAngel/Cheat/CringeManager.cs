using System;
using System.Collections.Generic;

namespace TerraAngel.Cheat
{
    public class CringeManager
    {
        private static Dictionary<int, Cringe> cringes = new Dictionary<int, Cringe>();
        private static List<Cringe>[] cringesAsTabs;
        private static List<Cringe> allCringes;

        static CringeManager()
        {
            cringesAsTabs = new List<Cringe>[Enum.GetValues<CringeTabs>().Length];
            for (int i = 0; i < cringesAsTabs.Length; i++)
            {
                cringesAsTabs[i] = new List<Cringe>(5);
            }

            allCringes = new List<Cringe>(30);
        }

        public static T GetCringe<T>() where T : Cringe => (T)GetCringe(typeof(T));
        public static void AddCringe<T>() where T : Cringe => AddCringe(typeof(T));
        public static void RemoveCringe<T>() where T : Cringe => RemoveCringe(typeof(T));

        public static Cringe GetCringe(Type type)
        {
            return cringes[type.MetadataToken];
        }
        public static void AddCringe(Type type)
        {
            if (!cringes.ContainsKey(type.MetadataToken))
            {
                Cringe cringe = (Cringe?)Activator.CreateInstance(type) ?? throw new NullReferenceException(type.Name);
                cringesAsTabs[(int)cringe.Tab].Add(cringe);
                allCringes.Add(cringe);
                cringes.Add(type.MetadataToken, cringe);
            }
        }
        public static void RemoveCringe(Type type)
        {
            if (cringes.ContainsKey(type.MetadataToken))
            {
                Cringe cringe = GetCringe(type);
                cringesAsTabs[(int)cringe.Tab].Remove(cringe);
                allCringes.Remove(cringe);
                cringes.Remove(type.MetadataToken);
            }
        }

        public static List<Cringe> GetCringeOfTab(CringeTabs tab)
        {
            return cringesAsTabs[(int)tab];
        }

        public static void SortTabs()
        {
            for (int i = 0; i < cringesAsTabs.Length; i++)
            {
                List<Cringe> tab = cringesAsTabs[i];
                tab.Sort((x, y) => x.Name.CompareTo(y.Name));
            }
        }

        public static void Update()
        {
            for (int i = 0; i < allCringes.Count; i++)
            {
                Cringe cringe = allCringes[i];
                cringe.Update();
            }
        }


        public static int ButcherDamage = 1000;

        public static bool AutoButcherHostileNPCs = false;

        public static bool NebulaSpam = false;
        public static int NebulaSpamPower = 100;

        public static bool[,] LoadedTileSections;
    }
}
