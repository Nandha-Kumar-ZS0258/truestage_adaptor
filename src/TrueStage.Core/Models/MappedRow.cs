using System.Collections.Generic;

namespace TrueStage.Core.Models;

public class MappedRow
{
    public int RowNumber { get; set; }
    public string? RawData { get; set; }
    public Dictionary<string, object?> Data { get; set; } = new();
    public string? RecordHash { get; set; }
    public bool HasError { get; set; }
    public string? ErrorMessage { get; set; }
}
