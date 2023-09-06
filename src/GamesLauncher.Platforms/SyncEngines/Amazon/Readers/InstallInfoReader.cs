using GamesLauncher.Platforms.SyncEngines.Amazon.Models;
using Microsoft.Data.Sqlite;
using System.Data;

namespace GamesLauncher.Platforms.SyncEngines.Amazon.Readers
{
    internal class InstallInfoReader : IAsyncDisposable
    {
        private static readonly string installInfoFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Amazon Games", "Data", "Games", "Sql", "GameInstallInfo.sqlite");

        private readonly SqliteConnection installInfoConnection = new($"Mode=ReadOnly;Data Source={installInfoFilePath}");

        public InstallInfoReader()
        {
            if (File.Exists(installInfoFilePath))
                installInfoConnection.Open();
        }

        public async IAsyncEnumerable<AmazonGameInstallInfo> GetInstalledGamesId()
        {
            if (installInfoConnection.State != ConnectionState.Open)
                yield break;

            using var command = installInfoConnection.CreateCommand();
            command.CommandText =
            @"
                    SELECT Id, ProductTitle
                    FROM DbSet
                    WHERE Installed = 1            
            ";
            using var reader = await command.ExecuteReaderAsync();

            while (reader.Read())
            {
                var id = reader.GetString(0);
                var title = reader.GetString(1);

                yield return new AmazonGameInstallInfo(id, title);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await installInfoConnection.DisposeAsync();
        }
    }
}
