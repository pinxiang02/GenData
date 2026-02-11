using Bogus; // Make sure you have this using statement
using DataGen_v1.Models;
using System.Text.Json;

namespace DataGen_v1.Services
{
    public class GeneratorService
    {
        private readonly Random _random = new Random();

        public string GeneratePayload(List<NodeConfig> allNodes)
        {
            // 1. Find Root Nodes (No Parent)
            var rootNodes = allNodes.Where(n => n.ParentId == null).ToList();
            var payload = new Dictionary<string, object>();

            foreach (var node in rootNodes)
            {
                payload[node.NodeName] = GenerateRecursive(node, allNodes);
            }

            // Add Timestamp
            payload["Timestamp"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        }

        private object GenerateRecursive(NodeConfig currentNode, List<NodeConfig> allNodes)
        {
            try
            {
                // CASE A: It's an Object/Container
                if (currentNode.DataType == DataTypeEnum.Object)
                {
                    // Find children of THIS node
                    var children = allNodes.Where(n => n.ParentId == currentNode.Id).ToList();
                    var childDict = new Dictionary<string, object>();

                    foreach (var child in children)
                    {
                        childDict[child.NodeName] = GenerateRecursive(child, allNodes);
                    }
                    return childDict;
                }

                // CASE B: It's a Value Field
                return GenerateValue(currentNode) ?? "null";
            }
            catch
            {
                return "Gen Error";
            }
        }

        private object? GenerateValue(NodeConfig config)
        {
            try
            {
                if (config.Mode == GenerationMode.FixedValue)
                {
                    return ParseType(config.FixedValue, config.DataType);
                }

                if (config.Mode == GenerationMode.RandomPick)
                {
                    if (string.IsNullOrEmpty(config.ValueList)) return null;
                    var options = config.ValueList.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    var selected = options[_random.Next(options.Length)];
                    return ParseType(selected, config.DataType);
                }

                if (config.Mode == GenerationMode.Range)
                {
                    double min = config.MinRange ?? 0;
                    double max = config.MaxRange ?? 100;

                    if (config.DataType == DataTypeEnum.Integer)
                        return _random.Next((int)min, (int)max + 1);
                    else
                    {
                        var val = min + (_random.NextDouble() * (max - min));
                        return Math.Round(val, 2);
                    }
                }

                // --- SMART DATA (BOGUS) LOGIC ---
                // You must have this block if you want the "Smart Data" dropdown to work
                if (config.Mode == GenerationMode.SmartData)
                {
                    var faker = new Faker();
                    return config.FixedValue switch
                    {
                        "FullName" => faker.Name.FullName(),
                        "FirstName" => faker.Name.FirstName(),
                        "LastName" => faker.Name.LastName(),
                        "Email" => faker.Internet.Email(),
                        "Phone" => faker.Phone.PhoneNumber(),
                        "Address" => faker.Address.FullAddress(),
                        "City" => faker.Address.City(),
                        "Country" => faker.Address.Country(),
                        "Company" => faker.Company.CompanyName(),
                        "Guid" => Guid.NewGuid().ToString(),
                        "DatePast" => faker.Date.Past(1).ToString("yyyy-MM-dd"),
                        "DateFuture" => faker.Date.Future(1).ToString("yyyy-MM-dd"),
                        _ => "Unknown Category"
                    };
                }
                // --------------------------------

                return "Config Error";
            }
            catch
            {
                return "Gen Error";
            }
        }

        private object? ParseType(string? value, DataTypeEnum type)
        {
            if (value == null) return null;
            return type switch
            {
                DataTypeEnum.Integer => int.TryParse(value, out int i) ? i : 0,
                DataTypeEnum.Double => double.TryParse(value, out double d) ? d : 0.0,
                DataTypeEnum.Boolean => bool.TryParse(value, out bool b) ? b : false,
                _ => value
            };
        }
    }
}