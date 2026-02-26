# /project:onboard-cu — Onboard a New Credit Union

You are the TrueStage CU Adaptor Generator.

## Input
Arguments: `$ARGUMENTS` (format: `./samples/{cu_id}_sample.csv {CU_ID}`)

Parse the arguments to extract:
- `SOURCE_FILE` — path to the CU's sample data file
- `CU_ID` — the Credit Union identifier (e.g., `CU_GAMMA`)

## Steps to Execute

### Step 1: Read Source File
Read the source file at `SOURCE_FILE`. Detect:
- File format (CSV / JSON / XML)
- Delimiter (if CSV)
- All column names present in the file
- Data types for each column (string, numeric, date, etc.)
- Sample values (first 3-5 rows)
- Any null/empty columns

### Step 2: Read Target Schema
Read `/schema/target.sql`. Identify all valid target columns for the `Members` table.

### Step 3: Read Past Patterns
Read `/.claude/memory/onboarded_cus.md` to check if any previously onboarded CU had similar column names or formats. Use those patterns for higher-confidence mapping.

### Step 4: Map Columns (with Confidence Scores)
Map each source column to a target column. Assign confidence:
- **HIGH 🟢** — strong name/semantic match (e.g., `FName` → `first_name`)
- **MEDIUM 🟡** — partial match, needs verification (e.g., `acct_bal` → `account_balance`)
- **LOW 🔴** — unclear, requires human decision

Display the mapping table:
```
┌─────────────────┬──────────────────┬────────────┬────────────────────────────────┐
│ Source Column   │ Target Column    │ Confidence │ Proposed Transform             │
├─────────────────┼──────────────────┼────────────┼────────────────────────────────┤
│ MemberID        │ source_member_id │ HIGH  🟢   │ trim                           │
│ FName           │ first_name       │ HIGH  🟢   │ trim                           │
│ DOB             │ date_of_birth    │ HIGH  🟢   │ date:MM/dd/yyyy                │
│ Bal             │ account_balance  │ MEDIUM 🟡  │ trim, to_decimal               │
│ Status          │ member_status    │ MEDIUM 🟡  │ value_map:A->ACTIVE,I->INACTIVE│
└─────────────────┴──────────────────┴────────────┴────────────────────────────────┘
```

**PAUSE HERE.** Ask the user to confirm or correct MEDIUM and LOW confidence mappings before proceeding.

### Step 5: Generate JSON Config
After confirmation, generate `/configs/{cu_id_lowercase}_mapping.json` following the structure in `CLAUDE.md`.

### Step 6: Generate Integration Test
Generate `/tests/TrueStage.Integration.Tests/{CuId}IntegrationTest.cs` following the pattern in `CuAlphaIntegrationTest.cs`.

### Step 7: Run Tests
```bash
dotnet test tests/TrueStage.Core.UnitTests/TrueStage.Core.UnitTests.csproj --no-build
dotnet test tests/TrueStage.Integration.Tests/TrueStage.Integration.Tests.csproj --no-build
```
Fix any failures. Rerun until all tests pass.

### Step 8: Update Memory
Append to `/.claude/memory/onboarded_cus.md`:
- CU ID and name
- Source format and delimiter
- Any quirks found (special date formats, value codes, prefix stripping, etc.)

### Step 9: Report Summary
Output a summary:
```
✅ CU {CU_ID} onboarded successfully
   Config: /configs/{cu_id_lower}_mapping.json
   Tests:  /tests/TrueStage.Integration.Tests/{CuId}IntegrationTest.cs
   Mapped: {N} columns
   Tests passed: {N}/{N}
```
