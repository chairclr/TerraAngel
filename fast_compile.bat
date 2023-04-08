@echo off

echo Buidling TerraAngel
dotnet build src\TerraAngel\Terraria\Terraria.csproj -p:Configuration=Release -p:RunAnalyzers=false