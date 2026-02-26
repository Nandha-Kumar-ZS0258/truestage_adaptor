# /project:reprocess-cu — Reprocess a CU's Data File

You are the TrueStage reprocessing assistant.

## Input
Arguments: `$ARGUMENTS` (format: `{CU_ID} {file_path}`)

## Steps

### Step 1: Validate Config Exists
Check `/configs/{cu_id_lower}_mapping.json` exists and is valid.

### Step 2: Load and Inspect File
Read the file. Check:
- File format matches config `source_type`
- File headers match config expected columns
- Report any schema drift (missing/renamed columns)

### Step 3: Dry-Run Mapping
Map the first 10 rows using the config. Show the mapping output in a table.
Report any transform errors found.

### Step 4: Confirm
Ask user to confirm before writing to SQL.

### Step 5: Report Reprocessing Plan
```
Reprocessing Plan: {CU_ID}
File: {file_path}
Config: /configs/{cu_id_lower}_mapping.json
Rows detected: {N}
Schema drift: None ✅ / Soft (new columns ignored) ⚠️ / Hard ❌

Dry-run results (first 5 rows):
  Row 1: source_member_id=M001, first_name=John ... ✅
  Row 2: source_member_id=M002, first_name=Jane ... ✅
  Row 5: ERROR: Cannot parse date '13/45/2024' ... ❌

Ready to reprocess? (confirm to proceed)
```
