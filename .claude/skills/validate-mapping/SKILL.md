---
name: validate-mapping
description: Validate a CU's mapping config JSON against the target schema and sample file. Reports errors and warnings. Usage: /validate-mapping CU_ALPHA
disable-model-invocation: true
allowed-tools: Read, Glob
---

You are the TrueStage mapping validator.

## Input
Arguments: `$ARGUMENTS` — CU_ID (e.g., `CU_ALPHA`) or path to a config JSON file.

## Steps

### Step 1: Load Config
Read `/configs/{cu_id_lower}_mapping.json`.

### Step 2: Validate Against Target Schema
Read `/schema/target.sql`. Check:
- Every `target_col` in mappings exists in the Members table
- `source_member_id` is present and has `"required": true`
- No duplicate `target_col` entries
- All `transform` values are valid: `trim`, `to_decimal`, `date:*`, `value_map:*`, `strip_prefix:*`, `default:*`, `none`, or chained combinations
- All `dq_rules` reference valid columns

### Step 3: Validate Against Sample File
If `/samples/{cu_id_lower}_sample.csv` exists:
- Read the file headers
- Check every non-null `source_col` in config exists in file headers
- Warn about file columns not in config (will be ignored at runtime)
- Apply transforms to first 3 rows, report any errors

### Step 4: Report
```
Validation Report: {CU_ID}
✅ All target columns valid
✅ source_member_id required=true
✅ No duplicate target columns
✅ All transforms recognized
⚠️  Source column 'MiddleName' in file has no mapping (will be ignored at runtime)
❌ ERROR: Transform 'to_integer' not supported — use 'none' or fix in config
```
