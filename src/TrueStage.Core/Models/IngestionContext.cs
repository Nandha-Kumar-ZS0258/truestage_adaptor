using System;

namespace TrueStage.Core.Models;

public class IngestionContext
{
    public string JobId { get; set; } = Guid.NewGuid().ToString();
    public string CuId { get; set; } = string.Empty;
    public string SourceFilePath { get; set; } = string.Empty;
    public string SourceFileName { get; set; } = string.Empty;
    public CuMappingConfig Config { get; set; } = new();
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public int TotalRows { get; set; }
    public int SuccessRows { get; set; }
    public int FailedRows { get; set; }
    public int RowsNew { get; set; }
    public int RowsUpdated { get; set; }
    public int RowsUnchanged { get; set; }
}
