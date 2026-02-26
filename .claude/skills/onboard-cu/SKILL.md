---
name: onboard-cu
description: Onboard a new Credit Union into TrueStage. Reads a sample source file, maps columns to the target schema with confidence scores, generates the JSON mapping config and integration tests, runs dotnet test, and updates memory files. Usage: /onboard-cu ./samples/cu_gamma_sample.csv CU_GAMMA
disable-model-invocation: true
allowed-tools: Read, Write, Edit, Bash, Glob, Grep
---

You are the TrueStage CU Adaptor Generator. Your job is to onboard a new Credit Union by generating a JSON mapping config and integration tests.

## Input
Arguments: `$ARGUMENTS` (format: `./samples/{cu_id}_sample.csv {CU_ID}`)

Parse the arguments:
- `SOURCE_FILE` — path to the CU's sample data file
- `CU_ID` — the Credit Union identifier (e.g., `CU_GAMMA`)

## Steps to Execute

### Step 1: Read Source File
Read `SOURCE_FILE`. Detect:
- File format (CSV / JSON / XML) and delimiter (if CSV)
- All column names present
- Data types for each column (string, numeric, date, etc.)
- Sample values (first 3–5 rows)
- Any discrete value columns (e.g., status codes)

### Step 2: Read Target Schema
Read `/schema/target.sql`. Note all valid target columns for the `Members` table.

### Step 3: Read Past Patterns
Read `/.claude/memory/onboarded_cus.md` to reuse known column name conventions and transform patterns.

### Step 4: Map Columns — Show Confidence Table
Map each source column to a target column. Show this table and **PAUSE for confirmation**:

```
┌─────────────────┬──────────────────┬────────────┬────────────────────────────────┐
│ Source Column   │ Target Column    │ Confidence │ Proposed Transform             │
├─────────────────┼──────────────────┼────────────┼────────────────────────────────┤
│ MemberID        │ source_member_id │ HIGH  🟢   │ trim                           │
│ FName           │ first_name       │ HIGH  🟢   │ trim                           │
│ Bal             │ account_balance  │ MEDIUM 🟡  │ trim, to_decimal               │
│ Status          │ member_status    │ MEDIUM 🟡  │ value_map:A->ACTIVE,I->INACTIVE│
└─────────────────┴──────────────────┴────────────┴────────────────────────────────┘
```

Confidence rules:
- **HIGH 🟢** — strong name/semantic match (FName→first_name, DOB→date_of_birth)
- **MEDIUM 🟡** — partial match, needs verification
- **LOW 🔴** — unclear, requires human input

**Ask the user to confirm or correct MEDIUM/LOW rows before continuing.**

### Step 5: Generate JSON Config
After confirmation, write `/configs/{cu_id_lower}_mapping.json`.

Structure:
```json
{
  "cu_id": "CU_GAMMA",
  "cu_name": "...",
  "source_type": "CSV",
  "adapter_version": "1.0",
  "target_table": "Members",
  "file_delimiter": ",",
  "has_header": true,
  "date_default_format": "MM/dd/yyyy",
  "mappings": [
    { "source_col": "...", "target_col": "source_member_id", "transform": "trim", "required": true },
    ...
    { "source_col": null, "target_col": "as_of_date", "transform": "default:utcnow" }
  ],
  "dq_rules": [
    { "column": "source_member_id", "rule": "not_null", "action": "log_and_skip" },
    { "column": "account_balance",  "rule": "positive",  "action": "log_and_skip" },
    { "column": "date_of_birth",    "rule": "valid_date", "action": "log_and_skip" }
  ]
}
```

### Step 6: Generate Integration Test
Write `/tests/TrueStage.Integration.Tests/{CuId}IntegrationTest.cs` following the pattern in `CuAlphaIntegrationTest.cs`.

### Step 7: Run Tests
```bash
dotnet build TrueStage.Adaptors.sln --no-incremental
dotnet test TrueStage.Adaptors.sln --logger "console;verbosity=minimal"
```
Fix any failures. Rerun until all tests pass.

### Step 8: Update Memory
Append to `/.claude/memory/onboarded_cus.md`:
- CU ID, name, source format, delimiter, date format, status code patterns, any quirks

### Step 9: Report
```
✅ CU {CU_ID} onboarded successfully
   Config:  /configs/{cu_id_lower}_mapping.json
   Tests:   /tests/TrueStage.Integration.Tests/{CuId}IntegrationTest.cs
   Mapped:  {N} columns
   Tests:   {N}/{N} passing
```
