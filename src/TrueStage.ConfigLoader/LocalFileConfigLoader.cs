using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TrueStage.Core.Interfaces;
using TrueStage.Core.Models;

namespace TrueStage.ConfigLoader;

/// <summary>
/// Loads mapping config from local filesystem — used for POC / local dev.
/// </summary>
public class LocalFileConfigLoader : IConfigLoader
{
    private readonly string _configsDirectory;

    public LocalFileConfigLoader(string configsDirectory)
    {
        _configsDirectory = configsDirectory;
    }

    public async Task<CuMappingConfig> LoadConfigAsync(string cuId)
    {
        var filePath = Path.Combine(_configsDirectory, $"{cuId.ToLower()}_mapping.json");

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Config file not found for CU '{cuId}': {filePath}");

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<CuMappingConfig>(json)
            ?? throw new InvalidOperationException($"Failed to deserialize config for CU '{cuId}'");
    }
}
