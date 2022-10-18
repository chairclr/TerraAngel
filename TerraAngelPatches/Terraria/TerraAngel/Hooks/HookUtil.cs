using System;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace TerraAngel.Hooks;

public class HookUtil
{
    public static MethodInfo GetMethod(Type type, string methodName)
    {
        return type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
    }
    public static MethodInfo GetMethod(Type type, string methodName, params Type[] parameterTypes)
    {
        return type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, parameterTypes, null);
    }
    public static IDetour HookGen(MethodInfo from, Delegate to)
    {
        IDetour detour = new Hook(from, to);
        detour.Apply();
        return detour;
    }
    public static IDetour HookGen(Delegate from, Delegate to)
    {
        IDetour detour = new Hook(from, to);
        detour.Apply();
        return detour;
    }
    public static IDetour HookGen<T>(string from, Delegate to)
    {
        IDetour detour = new Hook(GetMethod(typeof(T), from), to);
        detour.Apply();
        return detour;
    }
    public static IDetour HookGen<T>(string from, Delegate to, params Type[] parameterTypes)
    {
        IDetour detour = new Hook(GetMethod(typeof(T), from, parameterTypes), to);
        detour.Apply();
        return detour;
    }
    public static IDetour HookGen(Type fromType, string from, Delegate to)
    {
        IDetour detour = new Hook(GetMethod(fromType, from), to);
        detour.Apply();
        return detour;
    }
    public static IDetour HookGen(Type fromType, string from, Delegate to, params Type[] parameterTypes)
    {
        IDetour detour = new Hook(GetMethod(fromType, from, parameterTypes), to);
        detour.Apply();
        return detour;
    }
}
