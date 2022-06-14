@echo off

if exist C:\Program Files (x86)\Steam\steamapps\common\Terraria\
(
    echo Building TerraAngelSetup
    git submodule update --init --remote --recursive > NUL
    dotnet build TerraAngelSetup\TerraAngelSetup\TerraAngelSetup.csproj -c=Release > NUL

    echo Running TerraAngelSetup
    TerraAngelSetup\TerraAngelSetup\bin\Release\net6.0\TerraAngelSetup.exe -auto -patchinput TerraAngelPatches\

    echo Buidling TerraAngel as x86
    dotnet build src\TerraAngel\Terraria\Terraria.csproj -p:Configuration=Release;Platform=x86 > build_log_x86.txt

    echo Copying Terraria's content 
    xcopy "C:\Program Files (x86)\Steam\steamapps\common\Terraria\Content" src\TerraAngel\Terraria\bin\x86\Release\net6.0 > NUL

    echo Finished building! Your build should be in src\TerraAngel\Terraria\bin\x86\Release\net6.0
)
else
(
    echo "C:\Program Files (x86)\Steam\steamapps\common\Terraria\" does not exist, please specify a valid Terraria install directory
)