
<h1 align="center">
TerraAngel
</h1>
<p align="center">
TerraAngel is a modern and heavily featured utility client for Terraria.
</p>
<br>

<h2>
Installation
</h2>

To install, clone the github repo

```git clone https://github.com/CEO-Chair/TerraAngel.git --recursive```

To quickstart as x64, run `fast_start_x64.bat`, and as x86 `fast_start_x86.bat`

Or, manually build `TerraAngelSetup` and run `TerraAngelSetup.exe -auto` or `TerraAngelSetup.exe -decompile -patch` and build TerraAngel, and then run `TerraAngelSetup.exe -prebuild`

<h2>
Development
</h2>

After installing the client, you can edit the source code of the client in `src/TerraAngel/Terraria`

Run `fast_diff.bat` to create patches based on your changes

<h2>
Main features
</h2>

<h3>
Features for Terraria developers
</h3>

- Freecam
- Visual utilites
   - View player hitboxes
   - View NPC hitboxes and visualize NPC lag
   - View projectile hitboxes
   - View tile section borders
   - Disable dust and gore
- Interactive C# execution engine (aka [REPL](https://en.wikipedia.org/wiki/Read%E2%80%93eval%E2%80%93print_loop))
  - Real time auto-completion
- Net message debugger
  - Logging send and received
  - Stack traces of packets that are sent
  - Send messages with custom values and generate NetMessage.SendData calls
- Plugin system that supports hot-reloading
- Supports building x64 and x86

<h3>
Other useful features
</h3>

- Complete re-write of the chat UI
- Anti-Hurt/Godmode
- Fullbright
- Noclip
- Item browser
- Revealing the map
- Tools for butchering NPCs
- Actually good looking and customizable UI
- World edit 
  - Tile brush
    - Basic tile manipulation
    - Basic liquid manipluation
- Switch from the decades old Microsoft XNA framework to use FNA (significant performance boost)
- Viewing other players inventories
- Many bugfixes for vanilla

<h2>
Planned features
</h2>

- Re-write of the multiplayer UI
- Aimbot/Enemy prediction
- World edit features for cut-copy-paste
- More customization options for the clients UI
- Syntax highlighting for the C# [REPL](https://en.wikipedia.org/wiki/Read%E2%80%93eval%E2%80%93print_loop)
- Better completion for the console
- More console commands
  - Add keybind command to bind a console input to a key


<h2>
Contributing
</h2>


To contribute, open a pull request and I will review it and accept the PR if it suitable.

<h2>
Questions?
</h2>


Open an issue!