# TrueStage Data Integration Platform

A config-driven ETL adaptor platform for onboarding Credit Unions (CUs) into a centralized SQL database. Intelligence happens at onboarding time (Claude Code generates the adaptor). Mechanical execution happens at runtime (Azure Functions run it).

---

## How It Works

```
ONBOARDING TIME (once per CU)
  Developer runs /onboard-cu with a sample file
        ↓
  Claude Code reads file, maps columns, shows confidence table
        ↓
  Developer confirms mapping
        ↓
  Claude Code generates: JSON config + integration tests
        ↓
  dotnet test passes → PR merged → CI/CD deploys config

RUNTIME (every month, per CU)
  CU drops file → Azure Blob /incoming/{cu_id}/
        ↓
  Event Grid fires → Service Bus buffers
        ↓
  FileRouterFunction: loads JSON config for that CU
        ↓
  Schema drift check (hard fail or soft warn)
        ↓
  Raw file archived to /raw/{cu_id}/ (never deleted)
        ↓
  AdaptorEngine: maps columns, applies transforms, detects changes via hash
        ↓
  SQL: INSERT new / UPDATE changed / skip unchanged
        ↓
  Ingestion_Log closed, events fired, file moved to /processed/
```

**Key principle:** Adding CU #101 requires exactly 1 JSON config file + 1 test file. Zero C# changes. Zero redeployment.

---

## Prerequisites

- .NET 8 SDK
- Azure subscription (for production deployment)
- SQL Server (local or Azure SQL)
- Claude Code CLI (for onboarding new CUs)

---

## Quick Start

```bash
# Clone and build
git clone <repo>
cd truestage_adaptor
dotnet build TrueStage.Adaptors.sln

# Run all tests
dotnet test TrueStage.Adaptors.sln
```

**Tests:** 18 total — 13 unit tests, 4 integration tests, 1 engine test. All green.

---

## Onboarding a New CU (Claude Code)

Drop into Claude Code and run:

```
/onboard-cu ./samples/cu_gamma_sample.csv CU_GAMMA
```

Claude Code will:
1. Read the source file and detect columns + data types
2. Map source columns → target columns with confidence scores
3. Show a mapping table — you confirm or correct
4. Generate `/configs/cu_gamma_mapping.json`
5. Generate `/tests/TrueStage.Integration.Tests/CuGammaIntegrationTest.cs`
6. Run `dotnet test` and auto-fix any failures
7. Update `/.claude/memory/onboarded_cus.md` with CU patterns

**Total effort:** 1 command + confirm the mapping table.

### Other Claude Code Commands

| Command | What it does |
|---|---|
| `/onboard-cu ./samples/{cu_id}_sample.csv {CU_ID}` | Full CU onboarding — maps columns, generates config + tests, runs tests |
| `/validate-mapping CU_ALPHA` | Validates a config against target schema + sample file |
| `/run-regression` | Builds + runs all tests, auto-fixes failures |
| `/reprocess-cu CU_ALPHA ./samples/cu_alpha_sample.csv` | Dry-run preview then reprocess a file |

---

## Project Structure

```
truestage_adaptor/
│
├── CLAUDE.md                          ← Master instructions (auto-loaded by Claude Code)
│
├── schema/
│   └── target.sql                     ← Canonical SQL schema — reference only, never edit
│
├── configs/                           ← ONE JSON file per CU (grows with each onboarding)
│   ├── cu_alpha_mapping.json
│   ├── cu_beta_mapping.json
│   └── cu_gamma_mapping.json
│
├── samples/                           ← Sample source files per CU (for testing)
│   ├── cu_alpha_sample.csv
│   └── cu_beta_sample.csv
│
├── infra/                             ← Terraform — provision Azure once
│   ├── main.tf
│   └── variables.tf
│
├── src/
│   ├── TrueStage.Core/                ← Interfaces, readers, transformers, mappers
│   │   ├── Interfaces/                ← ISourceReader, IColumnMapper, ITransformer, ITargetWriter, IEventPublisher, IConfigLoader
│   │   ├── Models/                    ← CuMappingConfig, IngestionContext, MappedRow
│   │   ├── Readers/                   ← CsvSourceReader, JsonSourceReader
│   │   ├── Transformers/              ← TrimTransformer, DateTransformer, DecimalTransformer, ValueMapTransformer, ...
│   │   └── Mappers/                   ← ColumnMapper (with SHA-256 change detection)
│   │
│   ├── TrueStage.ConfigLoader/        ← Config loading
│   │   ├── BlobConfigLoader.cs        ← Loads from Azure Blob Storage (production)
│   │   ├── LocalFileConfigLoader.cs   ← Loads from local filesystem (dev/test)
│   │   └── ConfigValidator.cs         ← Validates config against target schema
│   │
│   ├── TrueStage.Engine/              ← Pipeline orchestration
│   │   ├── AdaptorEngine.cs           ← Full read → map → write pipeline
│   │   ├── SchemaDriftDetector.cs     ← Detects hard/soft column drift
│   │   ├── SqlTargetWriter.cs         ← INSERT/UPDATE/skip via Dapper
│   │   ├── ServiceBusEventPublisher.cs← Fires lifecycle events
│   │   └── NoOpEventPublisher.cs      ← No-op for local dev/testing
│   │
│   └── TrueStage.Function/            ← Azure Function entry points
│       ├── FileRouterFunction.cs      ← Triggered by 'file-arrived' Service Bus topic
│       └── AdaptorEngineFunction.cs   ← Triggered by 'ingestion-ready' Service Bus topic
│
├── tests/
│   ├── TrueStage.Core.UnitTests/      ← Transformer + mapper unit tests
│   ├── TrueStage.Engine.UnitTests/    ← Engine unit tests
│   └── TrueStage.Integration.Tests/   ← ONE test file per CU (auto-generated at onboarding)
│
└── .claude/
    ├── skills/
    │   ├── onboard-cu/SKILL.md        ← /onboard-cu command
    │   ├── validate-mapping/SKILL.md  ← /validate-mapping command
    │   ├── run-regression/SKILL.md    ← /run-regression command
    │   └── reprocess-cu/SKILL.md      ← /reprocess-cu command
    └── memory/
        ├── onboarded_cus.md           ← CU patterns (updated each onboarding)
        ├── known_patterns.md          ← Reusable transform patterns
        └── known_issues.md            ← Past failures + resolutions
```

---

## Mapping Config Format

Each CU has one JSON config file in `/configs/`. This is the only thing that differs per CU.

```json
{
  "cu_id": "CU_ALPHA",
  "cu_name": "Alpha Community Credit Union",
  "source_type": "CSV",
  "adapter_version": "1.0",
  "target_table": "Members",
  "file_delimiter": ",",
  "has_header": true,
  "date_default_format": "MM/dd/yyyy",
  "mappings": [
    { "source_col": "MemberID", "target_col": "source_member_id", "transform": "trim", "required": true },
    { "source_col": "FName",    "target_col": "first_name",       "transform": "trim" },
    { "source_col": "DOB",      "target_col": "date_of_birth",    "transform": "date:MM/dd/yyyy" },
    { "source_col": "Bal",      "target_col": "account_balance",  "transform": "trim, to_decimal" },
    { "source_col": "Status",   "target_col": "member_status",    "transform": "value_map:A->ACTIVE,I->INACTIVE,C->CLOSED" },
    { "source_col": null,       "target_col": "as_of_date",       "transform": "default:utcnow" }
  ],
  "dq_rules": [
    { "column": "source_member_id", "rule": "not_null", "action": "log_and_skip" },
    { "column": "account_balance",  "rule": "positive",  "action": "log_and_skip" },
    { "column": "date_of_birth",    "rule": "valid_date", "action": "log_and_skip" }
  ]
}
```

### Supported Transforms

| Transform | Example | Effect |
|---|---|---|
| `none` | `"transform": "none"` | Pass through unchanged |
| `trim` | `"transform": "trim"` | Strip leading/trailing whitespace |
| `to_decimal` | `"transform": "to_decimal"` | Parse decimal — auto-strips `$` and `,` |
| `date:{format}` | `"transform": "date:MM/dd/yyyy"` | Parse date with given format |
| `value_map:{pairs}` | `"transform": "value_map:A->ACTIVE,I->INACTIVE"` | Map discrete values |
| `strip_prefix:{char}` | `"transform": "strip_prefix:+"` | Strip a prefix character |
| `default:{value}` | `"transform": "default:utcnow"` | Use default when source is null |
| Chained | `"transform": "trim, to_decimal"` | Apply left-to-right |

---

## Target SQL Schema

Four tables — built once, shared by all CUs:

| Table | Purpose |
|---|---|
| `CU_Registry` | Master list of all onboarded CUs and their config paths |
| `Members` | Canonical member data — all CUs write here |
| `Ingestion_Log` | Audit trail per file: rows total/success/failed, SLA met |
| `Row_Error_Log` | Raw data of every failed row — no silent drops |

See [schema/target.sql](schema/target.sql) for full DDL.

---

## Azure Infrastructure

Provisioned once via Terraform in `/infra/`:

| Resource | Purpose |
|---|---|
| Azure Blob Storage | `/incoming/` `/raw/` `/configs/` `/processed/` `/failed/` containers |
| Event Grid | Watches `/incoming/` — fires on every new blob |
| Service Bus | 5 topics: `file-arrived`, `ingestion-ready`, `ingestion-started`, `ingestion-completed`, `ingestion-failed` |
| Azure Functions | `FileRouterFunction` + `AdaptorEngineFunction` — handle ALL CUs |
| Azure SQL | Hosts the 4 target tables |
| Key Vault | Stores all connection strings — zero hardcoded secrets |
| Application Insights | Structured logs + ingestion dashboard |

Concurrency is controlled via Service Bus `maxConcurrentCalls: 10` — never more than 10 CUs processing simultaneously.

---

## At Scale

```
100 CUs drop files simultaneously on Monday 9am
  → 100 messages queued in Service Bus
  → Batch 1 (CU_001–010): process in parallel
  → Batch 2 (CU_011–020): process in parallel
  → ...
  → All 100 done by ~8am
  → 0 new Azure Functions deployed
  → 0 C# code changed
```

Adding CU #101 = 1 JSON config + 1 test file + 1 SQL row.

---

## What Never Changes vs What Grows

**Built once — never changes:**
- `src/TrueStage.Core` — interfaces + transformers
- `src/TrueStage.Engine` — pipeline orchestration
- `src/TrueStage.Function` — Azure Functions
- `schema/target.sql` — SQL tables
- `infra/` — Azure infrastructure

**Grows with each new CU:**
- `configs/` — +1 JSON file per CU
- `tests/TrueStage.Integration.Tests/` — +1 test file per CU
- `CU_Registry` SQL table — +1 row per CU
- `.claude/memory/onboarded_cus.md` — +1 entry per CU

Invoke with	What it does
/onboard-cu ./samples/cu_gamma_sample.csv CU_GAMMA	Full CU onboarding — maps columns, generates config + tests, runs tests
/validate-mapping CU_ALPHA	Validates a config against target schema + sample file
/run-regression	Builds + runs all tests, auto-fixes failures
/reprocess-cu CU_ALPHA ./samples/cu_alpha_sample.csv	Dry-run preview then reprocess a file
