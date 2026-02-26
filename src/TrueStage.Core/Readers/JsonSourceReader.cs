using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TrueStage.Core.Interfaces;
using TrueStage.Core.Models;

namespace TrueStage.Core.Readers;

public class JsonSourceReader : ISourceReader
{
    public IEnumerable<Dictionary<string, string>> ReadRows(Stream stream, CuMappingConfig config)
    {
        using var doc = JsonDocument.Parse(stream);
        var root = doc.RootElement;

        var array = root.ValueKind == JsonValueKind.Array ? root : root.GetProperty("data");

        foreach (var element in array.EnumerateArray())
        {
            var row = new Dictionary<string, string>();
            foreach (var prop in element.EnumerateObject())
            {
                row[prop.Name] = prop.Value.ToString();
            }
            yield return row;
        }
    }
}
