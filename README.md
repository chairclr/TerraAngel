
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

You need to have [git](https://git-scm.com/download/win) installed

You need to have the [dotnet runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed 

First, open command prompt or some other terminal, and clone the repository

```git clone https://github.com/CEO-Chair/TerraAngel.git --recursive```

![image](https://user-images.githubusercontent.com/87276335/182042166-c967bcba-cd52-4372-ad75-3bc5faaac0ea.png)


Then run `fast_start.bat` and wait for it to finish, it will take a few minutes

![image](https://user-images.githubusercontent.com/87276335/182042235-9ce87d19-61ee-4636-b3ab-eee0ccb0e428.png)

If it finishes successfully, the client will be built in the src/TerraAngel/Terraria/bin/Release/net6.0/ folder

![image](https://user-images.githubusercontent.com/87276335/182298612-c9aa34a2-9df7-4047-9a4f-a465c95419a1.png)
![image](https://user-images.githubusercontent.com/87276335/182298616-e9e2299e-611c-4b7d-823e-b4d6ff828c42.png)


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
- Supports any CPU (x64 and x86)

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
- Projectile prediction
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
System Requirements
</h2>

Because TerraAngel uses FNA instead of XNA, not all devices are compatible

- Windows 7 or greater
- OpenGL 3.0+

<h2>
Contributing
</h2>


To contribute, open a pull request and I will review it and accept the PR if it suitable.

<h2>
Questions?
</h2>


Open an issue!
