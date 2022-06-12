@echo off

echo Building TerraAngelSetup
dotnet build TerraAngelSetup\TerraAngelSetup\TerraAngelSetup.csproj -c=Release > NUL

echo Running TerraAngelSetup
TerraAngelSetup\TerraAngelSetup\bin\Release\net6.0\TerraAngelSetup.exe -auto -patchinput TerraAngelSetup\TerraAngelSetup\Patches\TerraAngelPatches

echo Copying 
xcopy  src\TerraAngel\Terraria\bin\Release\net6.0 Build\ /E > NUL