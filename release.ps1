dotnet publish Flow.Launcher.Plugin.GamesLauncher -c Release -r win-x64 --no-self-contained
Compress-Archive -LiteralPath Flow.Launcher.Plugin.GamesLauncher/bin/Release/win-x64/publish -DestinationPath Flow.Launcher.Plugin.GamesLauncher/bin/GamesLauncher.zip -Force