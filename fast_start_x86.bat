@echo off

echo Building TerraAngelSetup
git submodule update --init --remote --recursive > NUL
dotnet build TerraAngelSetup\TerraAngelSetup\TerraAngelSetup.csproj -c=Release > NUL

echo Running TerraAngelSetup
TerraAngelSetup\TerraAngelSetup\bin\Release\net6.0\TerraAngelSetup.exe -auto -x86 -patchinput TerraAngelPatches\