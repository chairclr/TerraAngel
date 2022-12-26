using System;
using System.Collections.Generic;

namespace TerraAngel.Cheat;

public class CringeManager
{
    private static Dictionary<int, Cringe> LoadedCringes = new Dictionary<int, Cringe>();
    private static List<Cringe>[] CringesAsTabs;
    private static List<Cringe> AllCringes;

    static CringeManager()
    {
        CringesAsTabs = new List<Cringe>[Enum.GetValues<CringeTabs>().Length];
        for (int i = 0; i < CringesAsTabs.Length; i++)
        {
            CringesAsTabs[i] = new List<Cringe>(5);
        }

        AllCringes = new List<Cringe>(30);
    }

    public static T GetCringe<T>() where T : Cringe => (T)GetCringe(typeof(T));

    public static void AddCringe<T>() where T : Cringe => AddCringe(typeof(T));

    public static void RemoveCringe<T>() where T : Cringe => RemoveCringe(typeof(T));

    public static Cringe GetCringe(Type type)
    {
        return LoadedCringes[type.MetadataToken];
    }

    public static void AddCringe(Type type)
    {
        Cringe cringe = (Cringe?)Activator.CreateInstance(type) ?? throw new NullReferenceException(type.Name);
        CringesAsTabs[(int)cringe.Tab].Add(cringe);
        LoadedCringes.Add(type.MetadataToken, cringe);
        AllCringes.Add(cringe);
    }

    public static void RemoveCringe(Type type)
    {
        Cringe cringe = GetCringe(type);
        CringesAsTabs[(int)cringe.Tab].Remove(cringe);
        LoadedCringes.Remove(type.MetadataToken);
        AllCringes.Remove(cringe);
    }

    public static List<Cringe> GetCringeOfTab(CringeTabs tab)
    {
        return CringesAsTabs[(int)tab];
    }

    public static void SortTabs()
    {
        for (int i = 0; i < CringesAsTabs.Length; i++)
        {
            List<Cringe> tab = CringesAsTabs[i];
            tab.Sort((x, y) => x.Name.CompareTo(y.Name));
        }
    }

    public static void Update()
    {
        for (int i = 0; i < AllCringes.Count; i++)
        {
            Cringe cringe = AllCringes[i];
            cringe.Update();
        }
    }

    public static void Clear()
    {
        LoadedCringes.Clear();
        AllCringes.Clear();
        Array.Clear(CringesAsTabs);

        CringesAsTabs = new List<Cringe>[Enum.GetValues<CringeTabs>().Length];
        for (int i = 0; i < CringesAsTabs.Length; i++)
        {
            CringesAsTabs[i] = new List<Cringe>(5);
        }

        AllCringes = new List<Cringe>(30);
    }
}
