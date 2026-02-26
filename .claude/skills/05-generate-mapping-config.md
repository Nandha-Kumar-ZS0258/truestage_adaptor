# Skill: Generate Mapping Config JSON

Given confirmed column mappings (from skill 03) and transform rules (from skill 04):

## Generate `/configs/{cu_id_lower}_mapping.json`

Follow this structure exactly:
```json
{
  "cu_id": "{CU_ID}",
  "cu_name": "{CU_NAME}",
  "source_type": "CSV",
  "adapter_version": "1.0",
  "target_table": "Members",
  "file_delimiter": ",",
  "has_header": true,
  "date_default_format": "MM/dd/yyyy",
  "mappings": [
    {
      "source_col": "{source}",
      "target_col": "{target}",
      "transform": "{transform}",
      "required": false,
      "default_value": null
    }
  ],
  "dq_rules": [
    { "column": "source_member_id", "rule": "not_null", "action": "log_and_skip" },
    { "column": "account_balance",  "rule": "positive",  "action": "log_and_skip" },
    { "column": "date_of_birth",    "rule": "valid_date", "action": "log_and_skip" }
  ]
}
```

## Rules
- Always put `source_member_id` first in mappings, with `"required": true`
- Always include the `as_of_date` mapping with `"source_col": null, "transform": "default:utcnow"`
- Always include all 3 standard DQ rules
- Validate the generated JSON is valid (no trailing commas, no syntax errors)
