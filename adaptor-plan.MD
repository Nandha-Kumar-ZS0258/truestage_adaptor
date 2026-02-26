# TrueStage — Final Implementation Plan

---

## OVERVIEW

```
GOAL:
  When a CU sends data (CSV / JSON / XML) →
  it gets mapped, transformed, and ingested
  into a centralized SQL database automatically.

HOW:
  Claude Code generates the adaptor (once per CU)
  Azure Functions execute the adaptor (every data run)
  Service Bus coordinates everything safely
  Lambda Architecture preserves all data

TWO MODES:
  Onboarding Time  → Claude Code does the intelligence
  Runtime          → Azure Functions do the execution
```

---

## PHASE 1: FOUNDATION
### Build The Shared C# Engine (Built Once, Never Changes)

```
TrueStage.Adaptors/
├── src/
│   ├── TrueStage.Core/
│   │   ├── Interfaces/
│   │   │   ├── ISourceReader.cs        → reads source file
│   │   │   ├── IColumnMapper.cs        → maps columns
│   │   │   ├── ITransformer.cs         → applies transforms
│   │   │   ├── ITargetWriter.cs        → writes to SQL
│   │   │   └── IEventPublisher.cs      → fires events
│   │   ├── Readers/
│   │   │   ├── CsvSourceReader.cs
│   │   │   ├── JsonSourceReader.cs
│   │   │   └── XmlSourceReader.cs
│   │   ├── Transformers/
│   │   │   ├── TrimTransformer.cs
│   │   │   ├── DateTransformer.cs
│   │   │   ├── DecimalTransformer.cs
│   │   │   ├── ValueMapTransformer.cs
│   │   │   ├── StripPrefixTransformer.cs
│   │   │   └── TransformerFactory.cs
│   │   ├── Writers/
│   │   │   └── SqlTargetWriter.cs      → Dapper bulk insert
│   │   └── Events/
│   │       └── ServiceBusEventPublisher.cs
│   │
│   ├── TrueStage.ConfigLoader/
│   │   ├── BlobConfigLoader.cs         → loads CU JSON config
│   │   └── ConfigValidator.cs          → validates config structure
│   │
│   ├── TrueStage.Engine/
│   │   └── AdaptorEngine.cs            → orchestrates full pipeline
│   │
│   └── TrueStage.Function/
│       ├── FileRouterFunction.cs       → detects file, loads config
│       ├── AdaptorEngineFunction.cs    → processes file
│       ├── BatchSchedulerFunction.cs   → controls batch concurrency
│       └── Program.cs                  → 12-factor config from env
│
└── tests/
    ├── TrueStage.Core.UnitTests/
    ├── TrueStage.Engine.UnitTests/
    ├── TrueStage.Integration.Tests/    → one file per CU (generated)
    └── TrueStage.Regression.Tests/     → runs all CUs after any change
```

**SOLID Rules Applied:**
- Every class has ONE job
- All dependencies injected via interfaces
- TransformerFactory resolves correct transformer — open for extension
- AdaptorEngine never knows which CU it's processing — config drives it

**12 Factor Rules Applied:**
- Zero hardcoded secrets — all from Azure Key Vault
- Stateless functions — no shared memory between invocations
- Structured JSON logs to stdout → Application Insights

---

## PHASE 2: TARGET SQL SCHEMA
### The Canonical Destination (Built Once)

```sql
-- CU Registry — master list of all onboarded CUs
CREATE TABLE CU_Registry (
    cu_id           VARCHAR(50)  PRIMARY KEY,
    cu_name         VARCHAR(200) NOT NULL,
    status          VARCHAR(20)  DEFAULT 'ONBOARDING',  -- ONBOARDING/ACTIVE/INACTIVE
    adapter_version VARCHAR(10),
    config_path     VARCHAR(500),                        -- blob path to JSON config
    sla_hours       INT          DEFAULT 24,
    onboarded_date  DATETIME     DEFAULT GETUTCDATE()
);

-- Members — canonical member table
CREATE TABLE Members (
    member_id        BIGINT       IDENTITY PRIMARY KEY,
    cu_id            VARCHAR(50)  NOT NULL REFERENCES CU_Registry(cu_id),
    source_member_id VARCHAR(100) NOT NULL,
    first_name       VARCHAR(100),
    last_name        VARCHAR(100),
    date_of_birth    DATE,
    email            VARCHAR(255),
    phone            VARCHAR(20),
    member_status    VARCHAR(50),
    account_balance  DECIMAL(18,2),
    branch_code      VARCHAR(20),
    as_of_date       DATE,
    record_hash      VARCHAR(64),                        -- for change detection
    is_current       BIT          DEFAULT 1,
    ingested_at      DATETIME     DEFAULT GETUTCDATE(),
    adapter_version  VARCHAR(10),
    source_file      VARCHAR(500),
    UNIQUE (cu_id, source_member_id)
);

-- Ingestion Log — audit trail per file
CREATE TABLE Ingestion_Log (
    job_id        VARCHAR(100) PRIMARY KEY,
    cu_id         VARCHAR(50)  NOT NULL,
    source_file   VARCHAR(500) NOT NULL,
    status        VARCHAR(20)  NOT NULL,                 -- IN_PROGRESS/COMPLETED/FAILED
    total_rows    INT          DEFAULT 0,
    success_rows  INT          DEFAULT 0,
    failed_rows   INT          DEFAULT 0,
    rows_new      INT          DEFAULT 0,
    rows_updated  INT          DEFAULT 0,
    rows_unchanged INT         DEFAULT 0,
    sla_hours     INT,
    sla_met       BIT,
    started_at    DATETIME     DEFAULT GETUTCDATE(),
    completed_at  DATETIME,
    error_summary VARCHAR(MAX)
);

-- Row Error Log — row level failures
CREATE TABLE Row_Error_Log (
    error_id      BIGINT       IDENTITY PRIMARY KEY,
    job_id        VARCHAR(100) REFERENCES Ingestion_Log(job_id),
    cu_id         VARCHAR(50),
    row_number    INT,
    raw_data      VARCHAR(MAX),
    error_message VARCHAR(MAX),
    occurred_at   DATETIME     DEFAULT GETUTCDATE()
);
```

---

## PHASE 3: AZURE INFRASTRUCTURE
### The Runtime Platform (Provisioned Once via Terraform)

```
Azure Resources:

Blob Storage
  /incoming/{cu_id}/          ← CU drops files here
  /raw/{cu_id}/{date}/        ← Lambda raw archive (never deleted)
  /configs/                   ← all CU JSON adaptor configs
  /processed/{cu_id}/         ← files moved here after success
  /failed/{cu_id}/            ← files moved here after failure

Event Grid
  Watches /incoming/ container
  Fires on every new blob → no polling needed

Service Bus (5 Topics)
  file-arrived                ← Event Grid fires this
  ingestion-ready             ← FileRouter fires this
  ingestion-started           ← AdaptorEngine fires this
  ingestion-completed         ← AdaptorEngine fires this
  ingestion-failed            ← AdaptorEngine fires this

Azure Functions (2 only — regardless of CU count)
  FileRouterFunction          ← routes file to correct config
  AdaptorEngineFunction       ← processes any CU's file
  BatchSchedulerFunction      ← controls concurrency (max 10 at a time)

Azure SQL Database
  CU_Registry, Members, Ingestion_Log, Row_Error_Log

Azure Key Vault
  SQL connection string
  Service Bus connection string
  Blob storage connection string

Application Insights
  Structured logs from all functions
  Dashboard: ingestion rate, error rate, SLA status per CU
```

**Batch Concurrency Control (host.json):**
```json
{
  "extensions": {
    "serviceBus": {
      "maxConcurrentCalls": 10
    }
  }
}
```
Never more than 10 CUs processing simultaneously. Change one number to tune.

---

## PHASE 4: CLAUDE CODE SETUP
### The Adaptor Generator (Set Up Once, Used Forever)

```
.claude/
├── CLAUDE.md                          ← master instructions auto-loaded
├── skills/
│   ├── 01-detect-source-format.md    → CSV/JSON/XML detection
│   ├── 02-analyze-source-file.md     → columns, types, samples, nulls
│   ├── 03-map-columns.md             → source → target mapping
│   ├── 04-infer-transforms.md        → what transform each column needs
│   ├── 05-generate-mapping-config.md → writes cu_xxx_mapping.json
│   ├── 06-generate-tests.md          → writes CUXxxTests.cs
│   ├── 07-generate-regression.md     → updates RegressionSuite.cs
│   ├── 08-run-and-fix-tests.md       → dotnet test → fix → rerun loop
│   ├── 09-validate-config.md         → validates JSON vs target schema
│   ├── 10-detect-schema-drift.md     → compares file headers vs config
│   └── 11-update-memory.md           → logs CU details to memory files
│
├── commands/
│   ├── onboard-cu.md                 → /project:onboard-cu
│   ├── validate-mapping.md           → /project:validate-mapping
│   ├── run-regression.md             → /project:run-regression
│   └── reprocess-cu.md               → /project:reprocess-cu
│
└── memory/
    ├── onboarded_cus.md              → all CUs, quirks, patterns
    ├── known_patterns.md             → reusable transform patterns
    └── known_issues.md               → past failures + resolutions
```

**CLAUDE.md Key Rules:**
- Always reference /schema/target.sql as canonical destination
- Never generate new C# source — only JSON configs + tests
- SOLID: one responsibility per class, inject interfaces
- 12 Factor: no hardcoded secrets, stateless, structured logs
- Lambda: always archive raw file before any processing
- Eventing: always publish started/completed/failed events
- Tests always in /tests/ — never mixed with /src/

---

## PHASE 5: CU ONBOARDING FLOW
### What Happens Every Time A New CU Is Added

```
STEP 1: Business Registration
  Contract signed
        ↓
  Admin registers CU in portal
        ↓
  CU_Registry record created (status: ONBOARDING)
        ↓
  Terraform provisions blob containers + Key Vault entry
        ↓
  CU given SFTP/SAS credentials + submission guide


STEP 2: Sample File + Schema Discovery (Claude Code)
  CU drops sample file into /incoming/{cu_id}/
        ↓
  Developer runs in Claude Code:
  "Onboard the new CU files in ./incoming/cu_gamma/"
        ↓
  Claude Code Orchestrator:
    ├── Agent 1: reads sample file → detects format, columns, types
    ├── Agent 2: reads /schema/target.sql
    └── Agent 3: reads memory/onboarded_cus.md (past patterns)
        ↓
  Mapping Agent: maps source → target with confidence scores
        ↓
  Transform Agent: infers trim/date/decimal/value_map rules
        ↓
  PAUSE: Shows mapping table to developer
  ┌─────────────────┬──────────────────┬────────────┐
  │ Source Column   │ Target Column    │ Confidence │
  ├─────────────────┼──────────────────┼────────────┤
  │ mem_no          │ member_id        │ HIGH  🟢   │
  │ f_name          │ first_name       │ HIGH  🟢   │
  │ dob             │ date_of_birth    │ HIGH  🟢   │
  │ acct_bal        │ account_balance  │ MEDIUM 🟡  │
  │ status          │ member_status    │ MEDIUM 🟡  │
  └─────────────────┴──────────────────┴────────────┘
  Developer confirms / corrects


STEP 3: Adaptor Generation (Claude Code)
  Claude Code generates:
    ✅ /configs/cu_gamma_mapping.json   ← adaptor instruction card
    ✅ /tests/.../CUGammaTests.cs       ← integration tests
    ✅ RegressionSuite.cs updated       ← regression entry added
    ✅ memory/onboarded_cus.md updated  ← CU pattern logged
        ↓
  Runs: dotnet build && dotnet test
        ↓
  Auto-fixes any failures
        ↓
  Opens PR: "feat: onboard CU_GAMMA adaptor"


STEP 4: Review + Go Live
  Developer reviews PR
        ↓
  Merges → CI/CD deploys config to blob
        ↓
  End-to-end test in STAGING passes
        ↓
  CU_Registry status: ONBOARDING → ACTIVE
        ↓
  CU notified — start sending production files

  WHAT CHANGED IN THE CODEBASE:
    ✅ /configs/cu_gamma_mapping.json   (new file)
    ✅ /tests/.../CUGammaTests.cs       (new file)
    ✅ RegressionSuite.cs               (updated)
    ❌ Zero C# source code changes
    ❌ Zero Azure Function changes
    ❌ Zero infrastructure changes
    ❌ Zero redeployment of engine
```

---

## PHASE 6: RUNTIME FLOW
### What Happens Every Month When CU Sends Data

```
CU drops file → /incoming/cu_gamma/members_20260401.csv
        ↓
Event Grid detects blob → fires FILE_ARRIVED event
        ↓
Service Bus buffers message (maxConcurrentCalls: 10)
        ↓
FileRouterFunction wakes up:
  - looks up CU_Registry → status ACTIVE ✅
  - loads /configs/cu_gamma_mapping.json
  - detects file format: CSV
  - starts SLA timer
  - creates Ingestion_Log job record
  - fires ingestion-ready event
        ↓
SCHEMA DRIFT CHECK:
  file headers vs config expected headers
  HARD drift (missing col, renamed col) → quarantine + alert ❌
  SOFT drift (new col added) → warn + continue ⚠️
  No drift → proceed ✅
        ↓
LAMBDA — RAW ARCHIVE (always first):
  copies file to /raw/cu_gamma/2026-04-01/members_20260401.csv
  raw file never deleted — always reprocessable
        ↓
LAMBDA — BATCH LAYER:
  full file written to Data Lake (historical record)
        ↓
LAMBDA — SPEED LAYER:
  AdaptorEngine processes row by row
  applies column mappings from config
  applies transforms (trim, date, decimal, value_map)
  computes record_hash per row
  compares with last month's hash:
    new record     → INSERT into Members SQL
    changed record → UPDATE Members SQL, version old
    unchanged      → skip (no write needed)
        ↓
DQ RULES RUN:
  member_id: not null, unique ✅
  account_balance: positive decimal ✅
  date_of_birth: valid date ✅
  failed rows → Row_Error_Log
        ↓
ROW COUNT RECONCILIATION:
  file rows == ingested rows ✅
        ↓
EVENTS FIRED:
  ingestion-completed:
  {
    cu_id: "CU_GAMMA",
    job_id: "job-20260401-cu-gamma",
    total_rows: 5000,
    success_rows: 4998,
    failed_rows: 2,
    rows_new: 45,
    rows_updated: 120,
    rows_unchanged: 4833,
    sla_met: true
  }
        ↓
Ingestion_Log job closed: COMPLETED ✅
File moved to /processed/cu_gamma/
```

---

## PHASE 7: WHAT HAPPENS AT 100+ CUs

```
Infrastructure:    SAME 2 Azure Functions handle all 100 CUs
Configs:           100 JSON files in /configs/ blob container
Concurrency:       Service Bus limits to 10 CUs at a time
Parallelism:       10 CUs process simultaneously, independently
Adding CU #101:    1 JSON config + 1 test file. Zero else changes.

Monday 9am — 100 CUs drop files simultaneously:
  100 messages → Service Bus queue
        ↓
  Batch 1 (6:00am): CU_001–CU_010 process in parallel
  Batch 2 (6:10am): CU_011–CU_020 process in parallel
  ...
  Batch 10 (7:30am): CU_091–CU_100 process in parallel
        ↓
  All 100 done by ~8am
  All events fired
  Dashboard shows all 100 CUs green ✅
```

---

## DELIVERY SEQUENCE

```
Week 1-2:   Phase 1 — Build TrueStage.Core + Engine + Functions
Week 3:     Phase 2 — Create target SQL schema
Week 3:     Phase 3 — Provision Azure infrastructure (Terraform)
Week 4:     Phase 4 — Set up Claude Code (CLAUDE.md + skills + commands)
Week 5:     Phase 5 — Onboard first CU manually as reference pattern
Week 5:     Phase 6 — Validate full runtime flow end to end
Week 6:     Phase 7 — Onboard 2nd and 3rd CU using Claude Code command
Week 7+:    Scale — each new CU = 1 Claude Code command + PR review
```

---

## WHAT NEVER CHANGES vs WHAT GROWS

```
BUILT ONCE — NEVER CHANGES:
  TrueStage.Core          C# interfaces + transformers + readers
  TrueStage.Engine        pipeline orchestration
  TrueStage.Function      Azure Functions (FileRouter + AdaptorEngine)
  Azure Infrastructure    blob, service bus, SQL, event grid
  Target SQL Schema       canonical tables
  CLAUDE.md               master instructions
  Skill files             reusable Claude Code capabilities

GROWS WITH EACH NEW CU:
  /configs/               +1 JSON file per CU
  /tests/Integration/     +1 test file per CU
  CU_Registry SQL table   +1 row per CU
  memory/onboarded_cus.md +1 entry per CU
```

---

## KEY PRINCIPLES APPLIED THROUGHOUT

| Principle | Where Applied |
|---|---|
| SOLID | TrueStage.Core interfaces, TransformerFactory, AdaptorEngine |
| 12 Factor | Key Vault secrets, stateless functions, structured logs |
| Unit Tests | /tests/ separate from /src/, generated per CU by Claude Code |
| Regression Testing | RegressionSuite runs all CUs after any change |
| Eventing | Service Bus topics: started/completed/failed per ingestion |
| Lambda Architecture | Raw archive → Batch layer → Speed layer → Serving layer |
| Reusable Components | One engine, one set of functions, config drives CU differences |