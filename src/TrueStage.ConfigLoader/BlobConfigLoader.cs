using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using TrueStage.Core.Interfaces;
using TrueStage.Core.Models;

namespace TrueStage.ConfigLoader;

public class BlobConfigLoader : IConfigLoader
{
    private readonly BlobContainerClient _containerClient;

    public BlobConfigLoader(string connectionString, string containerName = "configs")
    {
        _containerClient = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<CuMappingConfig> LoadConfigAsync(string cuId)
    {
        var blobName = $"{cuId.ToLower()}_mapping.json";
        var blobClient = _containerClient.GetBlobClient(blobName);

        var response = await blobClient.DownloadContentAsync();
        var json = response.Value.Content.ToString();

        var config = JsonSerializer.Deserialize<CuMappingConfig>(json)
            ?? throw new InvalidOperationException($"Failed to deserialize config for CU '{cuId}'");

        return config;
    }
}
