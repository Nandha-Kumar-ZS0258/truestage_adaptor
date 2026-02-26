# TrueStage Adaptor Generator — Master Instructions

## What This Repo Does
Generates CU (Credit Union) adaptors that map source data (CSV/JSON/XML) into a centralized SQL schema.
Claude Code handles intelligence at onboarding time. Azure Functions handle mechanical execution at runtime.

## Architecture
- **One shared C# engine** — never changes regardless of CU count
- **One JSON config per CU** — drives all CU-specific logic
- **No new C# code per CU** — only a new JSON config + test file

## Canonical Target Schema
Always reference `/schema/target.sql` as the mapping destination.

**Valid target columns for `Members` table:**
- `source_member_id` (required — maps to CU's member identifier)
- `first_name`
- `last_name`
- `date_of_birth`
- `email`
- `phone`
- `member_status`
- `account_balance`
- `branch_code`
- `as_of_date`

## Tech Stack
- C# .NET 8
- Azure Blob Storage SDK (`Azure.Storage.Blobs`)
- Dapper for SQL writes
- CsvHelper for CSV parsing
- xUnit + Moq for tests
- Azure Functions (isolated worker model)
- Azure Service Bus for eventing
- Terraform for infrastructure

## Naming Conventions
- Config file: `/configs/{cu_id_lowercase}_mapping.json`
- Integration test: `/tests/TrueStage.Integration.Tests/{CuId}IntegrationTest.cs`
- CU IDs are always UPPERCASE (e.g., `CU_ALPHA`, `CU_BETA`)

## Supported Transforms (TransformerFactory)
| Transform | Example | Effect |
|---|---|---|
| `none` | `"transform": "none"` | Pass through unchanged |
| `trim` | `"transform": "trim"` | Trim whitespace |
| `to_decimal` | `"transform": "to_decimal"` | Parse decimal, strips $, commas |
| `date:{format}` | `"transform": "date:MM/dd/yyyy"` | Parse date with given format |
| `value_map:{pairs}` | `"transform": "value_map:A->ACTIVE,I->INACTIVE"` | Map discrete values |
| `strip_prefix:{char}` | `"transform": "strip_prefix:$"` | Strip prefix character |
| `default:{value}` | `"transform": "default:utcnow"` | Use default if source is null |
| Chained | `"transform": "trim, to_decimal"` | Apply left-to-right |

## Rules — Always Apply
1. **Never generate new C# source code** for a new CU — only JSON configs + test files
2. **Always include** an `Ingestion_Log` entry on start and completion
3. **Always write** failed rows to `Row_Error_Log` — never silently drop a row
4. **Always compute** `record_hash` (SHA256) for change detection (new/updated/unchanged)
5. **Always archive** raw file to `/raw/{cu_id}/` before any processing (Lambda raw layer)
6. **Always fire** Service Bus events: `ingestion-started` → `ingestion-completed` or `ingestion-failed`
7. **Zero hardcoded secrets** — all connection strings from Azure Key Vault / environment variables
8. **SOLID**: one class, one responsibility; all dependencies via interfaces
9. **12 Factor**: stateless functions, structured JSON logs to stdout
10. Tests always in `/tests/` — never mixed with `/src/`

## Onboarding a New CU
Run the slash command:
```
/onboard-cu ./samples/{cu_id}_sample.csv {CU_ID}
```

Or manually:
1. Read the source sample file
2. Map source columns → target columns (show confidence table)
3. Infer transform rules
4. Generate `/configs/{cu_id_lowercase}_mapping.json`
5. Generate `/tests/TrueStage.Integration.Tests/{CuId}IntegrationTest.cs`
6. Run `dotnet test` and fix failures
7. Update `/tests/TrueStage.Integration.Tests/RegressionSuite.cs`
8. Update `/.claude/memory/onboarded_cus.md`

## Project Structure
```
TrueStage.Adaptors/
├── CLAUDE.md                          ← you are here
├── schema/target.sql                  ← canonical SQL schema (read-only reference)
├── configs/                           ← ONE JSON config per CU (grows with each onboarding)
│   ├── cu_alpha_mapping.json
│   └── cu_beta_mapping.json
├── samples/                           ← sample source files per CU (for testing)
├── infra/                             ← Terraform Azure infrastructure
├── src/
│   ├── TrueStage.Core/                ← interfaces, readers, transformers, mappers
│   ├── TrueStage.ConfigLoader/        ← JSON config loading (blob + local)
│   ├── TrueStage.Engine/              ← pipeline orchestration + SQL writer + events
│   └── TrueStage.Function/            ← Azure Function entry points
└── tests/
    ├── TrueStage.Core.UnitTests/
    ├── TrueStage.Engine.UnitTests/
    └── TrueStage.Integration.Tests/   ← ONE test file per CU (generated)
```
