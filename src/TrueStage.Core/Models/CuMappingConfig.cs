using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrueStage.Core.Models;

public class CuMappingConfig
{
    [JsonPropertyName("cu_id")]
    public string CuId { get; set; } = string.Empty;

    [JsonPropertyName("cu_name")]
    public string CuName { get; set; } = string.Empty;

    [JsonPropertyName("source_type")]
    public string SourceType { get; set; } = "CSV"; // CSV, JSON, XML

    [JsonPropertyName("adapter_version")]
    public string AdapterVersion { get; set; } = "1.0";

    [JsonPropertyName("target_table")]
    public string TargetTable { get; set; } = "Members";

    [JsonPropertyName("file_delimiter")]
    public string FileDelimiter { get; set; } = ",";

    [JsonPropertyName("has_header")]
    public bool HasHeader { get; set; } = true;

    [JsonPropertyName("date_default_format")]
    public string DateDefaultFormat { get; set; } = "MM/dd/yyyy";

    [JsonPropertyName("mappings")]
    public List<ColumnMapping> Mappings { get; set; } = new();

    [JsonPropertyName("dq_rules")]
    public List<DqRule> DqRules { get; set; } = new();
}

public class ColumnMapping
{
    [JsonPropertyName("source_col")]
    public string? SourceCol { get; set; }

    [JsonPropertyName("target_col")]
    public string TargetCol { get; set; } = string.Empty;

    [JsonPropertyName("transform")]
    public string Transform { get; set; } = "none";

    [JsonPropertyName("required")]
    public bool Required { get; set; } = false;

    [JsonPropertyName("default_value")]
    public string? DefaultValue { get; set; }
}

public class DqRule
{
    [JsonPropertyName("column")]
    public string Column { get; set; } = string.Empty;

    [JsonPropertyName("rule")]
    public string Rule { get; set; } = string.Empty; // not_null, positive, valid_date, unique

    [JsonPropertyName("action")]
    public string Action { get; set; } = "log_and_skip"; // log_and_skip, fail_batch
}
