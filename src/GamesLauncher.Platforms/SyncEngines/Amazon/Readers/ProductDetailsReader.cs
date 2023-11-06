using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

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
                var json = reader.GetString(0);

                var jsonProductIconValue = JsonConvert.DeserializeObject<JObject>(json, new JsonSerializerSettings
                {
                    Error = delegate (object? sender, ErrorEventArgs args) // When malfunctioned JSON return null
                    {
                        args.ErrorContext.Handled = true;
                    }
                })
                ?.Value<string?>("ProductIconUrl");

                if (jsonProductIconValue == null)
                    continue;

                return jsonProductIconValue;
            }

            return fallbackIcon;
        }

        public async ValueTask DisposeAsync()
        {
            await productDetailsConnection.DisposeAsync();
        }
    }
}
