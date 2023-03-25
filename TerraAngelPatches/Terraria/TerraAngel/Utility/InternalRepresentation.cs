using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TerraAngel.Utility;

public class InternalRepresentation
{
    public static readonly Dictionary<int, FieldInfo> ItemIDFields = GetIDEnumMapping<ItemID>();

    public static readonly Dictionary<int, FieldInfo> PrefixIDFields = GetIDEnumMapping<PrefixID>();

    public static readonly Dictionary<int, FieldInfo> ProjectileIDFields = GetIDEnumMapping<ProjectileID>();

    // AmmoID is a special case because all of its fields are static int
    public static readonly Dictionary<int, FieldInfo> AmmoIDFields = 
        typeof(AmmoID).GetFields(BindingFlags.Public | BindingFlags.Static).Where(x => x.FieldType.IsValueType)
        .ToDictionary(x => UnboxStaticFieldToInt(x), x => x);

    public static readonly Dictionary<int, FieldInfo> NPCIDFields = GetIDEnumMapping<NPCID>();

    public static readonly Dictionary<int, FieldInfo> BuffIDFields = GetIDEnumMapping<BuffID>();

    public static readonly Dictionary<int, FieldInfo> MessageIDFields = GetIDEnumMapping<MessageID>();

    public static readonly Dictionary<int, FieldInfo> TileIDFields = GetIDEnumMapping<TileID>();

    public static readonly Dictionary<int, FieldInfo> WallIDFields = GetIDEnumMapping<WallID>();

    public static readonly Dictionary<int, FieldInfo> PaintIDFields = GetIDEnumMapping(typeof(PaintID));

    public static string GetItemIDName(int type)
    {
        if (ItemIDFields.TryGetValue(type, out FieldInfo? field))
        {
            return field.Name;
        }

        return "Invalid";
    }

    public static string GetPrefixIDName(int type)
    {
        if (PrefixIDFields.TryGetValue(type, out FieldInfo? field))
        {
            return field.Name;
        }

        return "Invalid";
    }

    public static string GetProjectileIDName(int type)
    {
        if (ProjectileIDFields.TryGetValue(type, out FieldInfo? field))
        {
            return field.Name;
        }

        return "Invalid";
    }

    public static string GetAmmoIDName(int type)
    {
        if (AmmoIDFields.TryGetValue(type, out FieldInfo? field))
        {
            return field.Name;
        }

        return "Invalid";
    }

    public static string GetNPCIDName(int type)
    {
        if (NPCIDFields.TryGetValue(type, out FieldInfo? field))
        {
            return field.Name;
        }

        return "Invalid";
    }

    public static string GetBuffIDName(int type)
    {
        if (BuffIDFields.TryGetValue(type, out FieldInfo? field))
        {
            return field.Name;
        }

        return "Invalid";
    }

    public static string GetMessageIDName(int type)
    {
        if (MessageIDFields.TryGetValue(type, out FieldInfo? field))
        {
            return field.Name;
        }

        return "Invalid";
    }

    public static string GetTileIDName(int type)
    {
        if (TileIDFields.TryGetValue(type, out FieldInfo? field))
        {
            return field.Name;
        }

        return "Invalid";
    }

    public static string GetWallIDName(int type)
    {
        if (WallIDFields.TryGetValue(type, out FieldInfo? field))
        {
            return field.Name;
        }

        return "Invalid";
    }

    public static string GetPaintIDName(int type)
    {
        if (PaintIDFields.TryGetValue(type, out FieldInfo? field))
        {
            return field.Name;
        }

        return "Invalid";
    }

    private static IEnumerable<FieldInfo> GetPublicValueTypeFields<T>()
    {
        return typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static).Where(x => x.FieldType.IsValueType && x.IsLiteral && !x.IsInitOnly);
    }

    private static IEnumerable<FieldInfo> GetPublicValueTypeFields(Type t)
    {
        return t.GetFields(BindingFlags.Public | BindingFlags.Static).Where(x => x.FieldType.IsValueType && x.IsLiteral && !x.IsInitOnly);
    }

    private static Dictionary<int, FieldInfo> GetIDEnumMapping<T>()
    {
        return GetPublicValueTypeFields<T>().ToDictionary(x => UnboxRawFieldToInt(x), x => x);
    }

    private static Dictionary<int, FieldInfo> GetIDEnumMapping(Type t)
    {
        return GetPublicValueTypeFields(t).ToDictionary(x => UnboxRawFieldToInt(x), x => x);
    }

    private static int UnboxRawFieldToInt(FieldInfo field)
    {
        dynamic? dyn = field.GetRawConstantValue();

        if (dyn is null)
            return 0;

        return (int)dyn;
    }

    private static int UnboxStaticFieldToInt(FieldInfo field)
    {
        dynamic? dyn = field.GetValue(null);

        if (dyn is null)
            return 0;

        return (int)dyn;
    }
}
