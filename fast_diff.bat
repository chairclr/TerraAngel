@echo off

echo Building TerraAngelSetup
git submodule update --remote --recursive > NUL
dotnet build TerraAngelSetup\TerraAngelSetup\TerraAngelSetup.csproj -c=Release > NUL

echo Running TerraAngelSetup -diff
TerraAngelSetup\TerraAngelSetup\bin\Release\net6.0\TerraAngelSetup.exe -diff -patchinput TerraAngelPatches\