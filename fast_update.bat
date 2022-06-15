@echo off

echo Building TerraAngelSetup
git reset --hard HEAD > NUL
git pull > NUL
git submodule update --remote --recursive > NUL
dotnet build TerraAngelSetup\TerraAngelSetup\TerraAngelSetup.csproj -c=Release > NUL

echo Running TerraAngelSetup -patch
TerraAngelSetup\TerraAngelSetup\bin\Release\net6.0\TerraAngelSetup.exe -patch -patchinput TerraAngelPatches\