<h1 align="center">
Plugins
</h1>
<p align="center">
TerraAngel supports extensions in the form of plugins.
</p>
<br>

# Getting Started

## Plugin Loading

TerraAngel will recognize any .NET class library ending with `.TAPlugin.dll` in the `Plugins` folder as a plugin.

When a plugin is loaded, TerraAngel will look for a class in the plugin assembly that inherits from the `Plugin` class defined in `TerraAngel.PluginAPI`.

## Example Plugin

Create a .NET class library for .NET 6.0, with a name ending in .TAPlugin (eg., Example.TAPlugin)

This can be accompished from the CLI: `dotnet new classlib --name Example.TAPlugin --framework net6.0`

```cs
using TerraAngel;
using TerraAngel.Plugin;

namespace Example.TAPlugin;

public class ExamplePlugin : Plugin
{
    public override string Name => "Example Plugin";

    public ExamplePlugin(string path) : base(path)
    {

    }

    // Called once on load
    public override void Load()
    {

    }

    // Called once on unload
    public override void Unload()
    {

    }

    // Called every frame that the plugin is loaded
    public override void Update()
    {

    }
}
```

This requires a reference to `TerraAngelPluginAPI.dll` and `Terraria.dll`

It is also recommended that you add a reference to the following:
 - `Terraria.dll`
 - `TerraAngel.PluginAPI.dll`
 - `ReLogic.dll`
 - `ImGui.NET.dll`
 - `FNA.dll`

These files are located in the TerraAngel build directory. (`TerraAngel/Terraria/bin/Release/net6.0/`)

To install a plugin, copy the plugin DLL to the TerraAngel plugins directory.

# Other Examples

```cs
using TerraAngel;
using TerraAngel.Plugin;
using Terraria.DataStructures;

namespace Console.TAPlugin;

public class ConsoleExamplePlugin : Plugin
{
    public override string Name => "Console Example Plugin";

    public ConsoleExample(string path) : base(path)
    {

    }

    // Called once on load
    public override void Load()
    {
        // Add a command to the console
        ClientLoader.Console.AddCommand("kill_player",
            (x) =>
            {
                Main.LocalPlayer.KillMe(PlayerDeathReason.ByPlayer(Main.myPlayer), 1, 0);
            },
            "Kills the player");
    }

    // Called once on unload
    public override void Unload()
    {

    }

    // Called every frame that the plugin is loaded
    public override void Update()
    {
        
    }
}
```