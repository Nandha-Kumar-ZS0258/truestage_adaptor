# /project:validate-mapping — Validate a CU Mapping Config

You are the TrueStage mapping validator.

## Input
Arguments: `$ARGUMENTS` (format: `{CU_ID}` or path to config JSON)

## Steps

### Step 1: Load Config
Read `/configs/{cu_id_lower}_mapping.json`.

### Step 2: Validate Against Target Schema
Read `/schema/target.sql`. Check:
- Every `target_col` in mappings exists in the Members table
- `source_member_id` is present and marked `required: true`
- No duplicate `target_col` entries
- All `transform` values are valid (trim, to_decimal, date:*, value_map:*, strip_prefix:*, default:*, none)
- All `dq_rules` reference valid columns

### Step 3: Validate Against Sample File (if available)
If `/samples/{cu_id_lower}_sample.csv` exists:
- Read the file headers
- Check every `source_col` in config exists in the file headers
- Warn about columns in the file that are NOT in the config (unmapped columns)
- Apply transforms to sample rows and report any errors

### Step 4: Report
```
Validation Report: {CU_ID}
✅ All target columns valid
✅ source_member_id required=true
✅ No duplicate target columns
✅ All transforms recognized
⚠️  WARNING: Source column 'MiddleName' in file has no mapping (will be ignored)
❌ ERROR: Transform 'to_integer' is not supported. Use 'none' or check TransformerFactory.
```
