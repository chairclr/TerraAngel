@echo off

echo Buidling TerraAngel as x86
dotnet build src\TerraAngel\Terraria\Terraria.csproj -p:Configuration=Release;Platform=x86 > build_log_x86.txt