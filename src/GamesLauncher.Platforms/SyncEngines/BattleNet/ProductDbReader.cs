using System.Text;

namespace GamesLauncher.Platforms.SyncEngines.BattleNet
{
    internal record BattleNetProduct(string Uid, string ProductCode, string InstallPath, bool Installed);

    // Reads Battle.net Agent's product.db (protobuf: Database { repeated ProductInstall product_install = 1 }).
    // shortcut: hand-rolled protobuf reader for the few fields we need, switch to Google.Protobuf + .proto if more fields are needed.
    internal static class ProductDbReader
    {
        internal static List<BattleNetProduct> Read(byte[] data)
        {
            return Fields(data)
                .Where(x => x.Field == 1)
                .Select(x => ParseProductInstall(x.Bytes))
                .ToList();
        }

        // ProductInstall { uid = 1; product_code = 2; settings = 3 { install_path = 1 }; cached_product_state = 4 { base_product_state = 1 { installed = 1 } } }
        private static BattleNetProduct ParseProductInstall(byte[] data)
        {
            var install = Fields(data);
            var settings = Fields(Bytes(install, 3));
            var baseState = Fields(Bytes(Fields(Bytes(install, 4)), 1));

            return new BattleNetProduct(
                Uid: String(install, 1),
                ProductCode: String(install, 2),
                InstallPath: String(settings, 1),
                Installed: baseState.Any(x => x.Field == 1 && x.Varint != 0));
        }

        private static byte[] Bytes(List<(int Field, ulong Varint, byte[] Bytes)> fields, int field)
            => fields.FirstOrDefault(x => x.Field == field).Bytes ?? [];

        private static string String(List<(int Field, ulong Varint, byte[] Bytes)> fields, int field)
            => Encoding.UTF8.GetString(Bytes(fields, field));

        private static List<(int Field, ulong Varint, byte[] Bytes)> Fields(byte[] data)
        {
            var fields = new List<(int, ulong, byte[])>();
            var pos = 0;

            while (pos < data.Length)
            {
                var key = ReadVarint(data, ref pos);
                var field = (int)(key >> 3);

                switch (key & 7)
                {
                    case 0:
                        fields.Add((field, ReadVarint(data, ref pos), []));
                        break;
                    case 1:
                        pos += 8;
                        break;
                    case 2:
                        var length = (int)ReadVarint(data, ref pos);
                        fields.Add((field, 0, data[pos..(pos + length)]));
                        pos += length;
                        break;
                    case 5:
                        pos += 4;
                        break;
                    default:
                        throw new InvalidDataException($"Unsupported protobuf wire type {key & 7} in Battle.net product.db");
                }
            }

            return fields;
        }

        private static ulong ReadVarint(byte[] data, ref int pos)
        {
            ulong result = 0;
            for (var shift = 0; ; shift += 7)
            {
                var b = data[pos++];
                result |= (ulong)(b & 0x7F) << shift;
                if (b < 0x80)
                    return result;
            }
        }
    }
}
