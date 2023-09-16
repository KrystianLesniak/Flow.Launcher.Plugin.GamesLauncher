dotnet publish src/GamesLauncher -c Release -r win-x64 --no-self-contained -o publish/
Compress-Archive -Path publish/* -DestinationPath publish/GamesLauncher.zip -Force