@echo off

echo Building TerraAngelSetup
git submodule update > NUL
dotnet build TerraAngelSetup\TerraAngelSetup\TerraAngelSetup.csproj -c=Release > NUL

echo Running TerraAngelSetup -patch
TerraAngelSetup\TerraAngelSetup\bin\Release\net6.0\TerraAngelSetup.exe -patch -patchinput TerraAngelSetup\TerraAngelSetup\Patches\TerraAngelPatches