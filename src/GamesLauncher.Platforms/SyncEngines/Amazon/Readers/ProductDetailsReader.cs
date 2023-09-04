using Microsoft.Data.Sqlite;
using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GamesLauncher.Platforms.SyncEngines.Amazon.Readers
{
    internal class ProductDetailsReader : IAsyncDisposable
    {
        private static readonly string productDetailsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Amazon Games", "Data", "Games", "Sql", "ProductDetails.sqlite");

        private readonly string fallbackIcon = Path.Combine("Icons", "amazon.png");

        private readonly SqliteConnection productDetailsConnection = new($"Mode=ReadOnly;Data Source={productDetailsFilePath}");

        public ProductDetailsReader()
        {
            if (File.Exists(productDetailsFilePath))
                productDetailsConnection.Open();
        }

        public async Task<string> GetIconUrlById(string gameId)
        {
            if (productDetailsConnection.State != ConnectionState.Open)
                return fallbackIcon;

            await using var command = productDetailsConnection.CreateCommand();
            command.CommandText =
            @"
                 SELECT value
                 FROM game_product_info
                 WHERE key = ($id)       
            ";
            command.Parameters.AddWithValue("$id", gameId);

            await using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                await using var json = reader.GetStream(0);
                var jsonNode = (await JsonSerializer.DeserializeAsync<JsonNode>(json))?["ProductIconUrl"];

                if (jsonNode == null)
                    continue;

                return jsonNode.GetValue<string>();
            }

            return fallbackIcon;
        }

        public async ValueTask DisposeAsync()
        {
            await productDetailsConnection.DisposeAsync();
        }
    }
}
