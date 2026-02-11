using Npgsql; // Requires 'Npgsql' NuGet package
using System.Data;
using System.Text.Json;

namespace DataGen_v1.Services
{
    public class DbInsertService
    {
        // NEW: Fetch tables for the dropdown
        public async Task<List<string>> GetTablesAsync(string connectionString)
        {
            var tables = new List<string>();
            try
            {
                using var db = new NpgsqlConnection(connectionString);
                await db.OpenAsync(); // Async open

                using var cmd = db.CreateCommand();
                // Query to get all public tables in Postgres
                cmd.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' ORDER BY table_name;";

                using var reader = await cmd.ExecuteReaderAsync(); // Async read
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }
            catch
            {
                // Return empty list on connection failure
            }
            return tables;
        }

        // UPDATED: Async insert for Postgres only
        public async Task<(bool Success, string Message)> InsertPayloadAsync(string connectionString, string tableName, string jsonPayload)
        {
            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonPayload);
                if (data == null || data.Count == 0) return (false, "Empty Payload");

                var columns = data.Keys.ToList();
                var values = data.Values.ToList();

                // Quote columns for safety: "ColumnName"
                var colList = string.Join(", ", columns.Select(c => $"\"{c}\""));
                var paramList = string.Join(", ", columns.Select((_, i) => $"@p{i}"));
                var query = $"INSERT INTO \"{tableName}\" ({colList}) VALUES ({paramList})";

                using var db = new NpgsqlConnection(connectionString);
                await db.OpenAsync(); // Fixes CS1998

                using var cmd = db.CreateCommand();
                cmd.CommandText = query;

                for (int i = 0; i < values.Count; i++)
                {
                    var p = new NpgsqlParameter($"@p{i}", values[i] ?? DBNull.Value);

                    // Handle JsonElement to ensure correct DB types
                    if (values[i] is JsonElement je)
                    {
                        switch (je.ValueKind)
                        {
                            case JsonValueKind.String: p.Value = je.GetString(); break;
                            case JsonValueKind.Number:
                                if (je.TryGetInt32(out int iVal)) p.Value = iVal;
                                else p.Value = je.GetDouble();
                                break;
                            case JsonValueKind.True: p.Value = true; break;
                            case JsonValueKind.False: p.Value = false; break;
                            default: p.Value = DBNull.Value; break;
                        }
                    }
                    cmd.Parameters.Add(p);
                }

                await cmd.ExecuteNonQueryAsync(); // Fixes CS1998
                return (true, "Inserted");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}