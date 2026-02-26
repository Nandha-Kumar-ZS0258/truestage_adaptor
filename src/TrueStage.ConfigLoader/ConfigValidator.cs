using System;
using System.Collections.Generic;
using TrueStage.Core.Models;

namespace TrueStage.ConfigLoader;

public class ConfigValidator
{
    private static readonly HashSet<string> ValidTargetColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "source_member_id", "first_name", "last_name", "date_of_birth",
        "email", "phone", "member_status", "account_balance",
        "branch_code", "as_of_date"
    };

    public ValidationResult Validate(CuMappingConfig config)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(config.CuId))
            errors.Add("cu_id is required");

        if (config.Mappings == null || config.Mappings.Count == 0)
            errors.Add("At least one mapping is required");

        foreach (var mapping in config.Mappings ?? new())
        {
            if (string.IsNullOrWhiteSpace(mapping.TargetCol))
                errors.Add("Each mapping must have a target_col");

            if (!ValidTargetColumns.Contains(mapping.TargetCol))
                errors.Add($"Unknown target column: '{mapping.TargetCol}'");
        }

        return new ValidationResult(errors.Count == 0, errors);
    }
}

public record ValidationResult(bool IsValid, List<string> Errors);
