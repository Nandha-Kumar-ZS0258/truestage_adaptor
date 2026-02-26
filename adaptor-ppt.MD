import { useState } from "react";

const slides = [
  {
    id: 1,
    section: "Overview",
    title: "TrueStage Data Integration Platform",
    subtitle: "Credit Union Data Adaptor Architecture",
    type: "cover"
  },
  {
    id: 2,
    section: "Overview",
    title: "What Are We Building?",
    type: "problem",
    content: {
      problem: "100+ Credit Unions send data in different formats (CSV, JSON, XML) with different column names, date formats, and value codes.",
      goal: "One platform that automatically maps, transforms, and ingests any CU's data into a single centralized SQL database.",
      approach: "Config-driven adaptors — intelligence at onboarding time, mechanical execution at runtime."
    }
  },
  {
    id: 3,
    section: "Overview",
    title: "Two Modes of Operation",
    type: "two-modes",
    modes: [
      {
        title: "Onboarding Time",
        subtitle: "Once per CU",
        color: "#6366f1",
        icon: "🧠",
        items: ["Claude Code reads sample file", "Discovers schema & maps columns", "Generates adaptor config (JSON)", "Generates + runs tests", "Human reviews & approves", "CI/CD deploys config"]
      },
      {
        title: "Runtime",
        subtitle: "Every month per CU",
        color: "#10b981",
        icon: "⚙️",
        items: ["CU drops file in Azure Blob", "Azure Function detects file", "Loads adaptor config (JSON)", "Maps & transforms rows in C#", "Writes to SQL database", "Fires completion events"]
      }
    ]
  },
  {
    id: 4,
    section: "CU Onboarding",
    title: "CU Onboarding — Full Flow",
    type: "flow",
    steps: [
      { phase: "Phase 1", title: "Registration", color: "#6366f1", items: ["Contract signed", "Admin registers CU in portal", "CU_Registry record created (status: ONBOARDING)", "Terraform auto-provisions blob containers, Key Vault, Event Grid", "CU given SFTP/SAS credentials"] },
      { phase: "Phase 2", title: "Schema Discovery", color: "#f59e0b", items: ["CU drops sample file into /incoming/{cu_id}/", "Claude Code reads file + target schema", "Auto-maps columns with confidence scores", "Infers transforms (date, decimal, trim, value_map)", "Shows mapping table — human confirms"] },
      { phase: "Phase 3", title: "Adaptor Generation", color: "#10b981", items: ["Claude Code generates cu_xxx_mapping.json", "Generates integration tests (CUXxxTests.cs)", "Runs dotnet test — auto-fixes failures", "Updates regression suite", "Opens PR for human review"] },
      { phase: "Phase 4", title: "Go Live", color: "#ef4444", items: ["Developer reviews & merges PR", "CI/CD deploys config to blob storage", "End-to-end test in STAGING passes", "CU_Registry status: ONBOARDING → ACTIVE", "CU notified — start sending production files"] }
    ]
  },
  {
    id: 5,
    section: "CU Onboarding",
    title: "Schema Discovery — Who Does What",
    type: "three-players",
    players: [
      {
        name: "Azure Function",
        role: "The Coordinator",
        color: "#0ea5e9",
        icon: "⚡",
        does: ["Detects new file in blob", "Reads file bytes", "Sends to Claude Code", "Saves mapping proposal", "Notifies human reviewer"],
        doesnt: ["Understands the data", "Makes mapping decisions"]
      },
      {
        name: "Claude Code",
        role: "The Brain",
        color: "#6366f1",
        icon: "🧠",
        does: ["Reads file directly", "Detects column names & types", "Maps source → target columns", "Infers transform rules", "Assigns confidence scores", "Flags ambiguous columns", "Generates JSON config + tests"],
        doesnt: ["Deploy anything", "Approve mappings"]
      },
      {
        name: "Human Reviewer",
        role: "The Decision Maker",
        color: "#10b981",
        icon: "👤",
        does: ["Reviews mapping table", "Confirms MEDIUM confidence rows", "Fixes LOW confidence rows", "Approves final mapping", "Merges PR"],
        doesnt: ["Write any code", "Touch infrastructure"]
      }
    ]
  },
  {
    id: 6,
    section: "CU Onboarding",
    title: "Mapping Table — Human Review",
    type: "mapping-table",
    rows: [
      { source: "mem_no", target: "member_id", confidence: "HIGH", transform: "trim", status: "green" },
      { source: "f_name", target: "first_name", confidence: "HIGH", transform: "trim, uppercase_first", status: "green" },
      { source: "l_name", target: "last_name", confidence: "HIGH", transform: "trim", status: "green" },
      { source: "dob", target: "date_of_birth", confidence: "HIGH", transform: "date:MM/dd/yyyy", status: "green" },
      { source: "acct_bal", target: "account_balance", confidence: "MEDIUM", transform: "strip_prefix:$, strip_commas, to_decimal", status: "yellow" },
      { source: "status", target: "member_status", confidence: "MEDIUM", transform: "value_map: A→ACTIVE, I→INACTIVE, C→CLOSED", status: "yellow" },
      { source: "branch_cd", target: "branch_code", confidence: "LOW", transform: "trim", status: "red" }
    ]
  },
  {
    id: 7,
    section: "CU Onboarding",
    title: "What An Adaptor Is",
    type: "adaptor",
    analogy: "An adaptor is the instruction card that tells the shared engine how to translate one specific CU's data into your canonical SQL schema.",
    components: [
      { title: "cu_gamma_mapping.json", subtitle: "The Instruction Card", icon: "📋", color: "#6366f1", items: ["Column mappings (source → target)", "Transform rules (date, decimal, trim)", "Value mappings (A → ACTIVE)", "Data quality rules", "File format (CSV/JSON/XML)"] },
      { title: "CUGammaTests.cs", subtitle: "Proof It Works", icon: "✅", color: "#10b981", items: ["Tests each column mapping", "Tests each transform rule", "Tests value mappings", "Tests DQ rule enforcement", "Tests null handling"] },
      { title: "AdaptorEngine.cs", subtitle: "Shared Machine (built once)", icon: "⚙️", color: "#f59e0b", items: ["Reads any CU's JSON config", "Applies mappings mechanically", "Applies transforms", "Writes to SQL", "Same for ALL 100 CUs"] }
    ]
  },
  {
    id: 8,
    section: "CU Runtime",
    title: "CU Monthly Data Run — Full Flow",
    type: "runtime-flow",
    steps: [
      { icon: "📁", title: "File Arrives", desc: "CU drops file into /incoming/{cu_id}/", color: "#6366f1" },
      { icon: "⚡", title: "Event Grid Fires", desc: "Detects new blob automatically", color: "#0ea5e9" },
      { icon: "📬", title: "Service Bus Buffers", desc: "Queues message, max 10 CUs at a time", color: "#f59e0b" },
      { icon: "🔍", title: "FileRouter Runs", desc: "Looks up CU Registry, loads JSON config, starts SLA timer", color: "#8b5cf6" },
      { icon: "🔎", title: "Schema Drift Check", desc: "Compares file headers vs expected config", color: "#ef4444" },
      { icon: "💾", title: "Raw Archive", desc: "Copies original file to /raw/ — never deleted", color: "#64748b" },
      { icon: "🏭", title: "AdaptorEngine", desc: "Maps columns, applies transforms, detects changes via hash", color: "#10b981" },
      { icon: "🗄️", title: "SQL Written", desc: "New records INSERT, changed UPDATE, unchanged skip", color: "#06b6d4" },
      { icon: "📡", title: "Events Fired", desc: "IngestionCompleted published to Service Bus", color: "#f97316" },
      { icon: "✅", title: "Job Closed", desc: "Ingestion_Log updated, SLA checked, file moved to /processed/", color: "#10b981" }
    ]
  },
  {
    id: 9,
    section: "CU Runtime",
    title: "Schema Drift Detection",
    type: "drift",
    types: [
      { type: "Missing Column", example: "Expected: mem_no, f_name, dob\nGot: mem_no, f_name", action: "HARD FAIL", detail: "Quarantine file + alert team immediately", color: "#ef4444", icon: "🚫" },
      { type: "New Column Added", example: "Expected: mem_no, f_name\nGot: mem_no, f_name, middle_name", action: "SOFT WARNING", detail: "Process file, ignore new col, alert team", color: "#f59e0b", icon: "⚠️" },
      { type: "Column Renamed", example: "Expected: f_name\nGot: first_name", action: "HARD FAIL", detail: "Quarantine + trigger re-onboarding flow", color: "#ef4444", icon: "🚫" },
      { type: "Data Type Changed", example: "Expected: acct_bal as DECIMAL\nGot: '$5,200.00' as STRING", action: "HARD FAIL", detail: "Quarantine + alert data team", color: "#ef4444", icon: "🚫" }
    ]
  },
  {
    id: 10,
    section: "Azure Functions",
    title: "Azure Function Architecture",
    type: "functions",
    insight: "Only 2 Azure Functions handle ALL CUs — regardless of whether you have 5 or 500 CUs.",
    functions: [
      {
        name: "FileRouterFunction",
        trigger: "Service Bus — file-arrived topic",
        color: "#6366f1",
        responsibilities: ["Reads cu_id from blob path", "Looks up CU_Registry (is CU active?)", "Loads /configs/{cu_id}_mapping.json", "Detects file format (CSV/JSON/XML)", "Starts SLA timer", "Creates Ingestion_Log job record", "Fires ingestion-ready event"]
      },
      {
        name: "AdaptorEngineFunction",
        trigger: "Service Bus — ingestion-ready topic",
        color: "#10b981",
        responsibilities: ["Archives raw file to /raw/ first", "Reads file row by row", "Applies column mappings from config", "Applies transform rules from config", "Computes record_hash per row", "Detects new/changed/unchanged rows", "Bulk inserts to SQL via Dapper", "Fires ingestion-completed/failed event"]
      },
      {
        name: "BatchSchedulerFunction",
        trigger: "Timer — runs every 30 min",
        color: "#f59e0b",
        responsibilities: ["Controls batch concurrency", "maxConcurrentCalls: 10", "Never more than 10 CUs at once", "Prevents SQL overload", "Handles 100 CUs in controlled waves"]
      }
    ]
  },
  {
    id: 11,
    section: "Azure Functions",
    title: "How One Function Handles All CUs",
    type: "one-function",
    code: [
      { label: "Message arrives at FileRouter:", value: '{ "cu_id": "CU_GAMMA", "file_path": "incoming/cu_gamma/members.csv" }' },
      { label: "FileRouter asks:", value: "What config does CU_GAMMA use?" },
      { label: "Loads:", value: "/configs/cu_gamma_mapping.json" },
      { label: "Next message:", value: '{ "cu_id": "CU_ALPHA", "file_path": "incoming/cu_alpha/accounts.csv" }' },
      { label: "Same function, loads:", value: "/configs/cu_alpha_mapping.json" }
    ],
    principle: "The function never has CU-specific logic. The config IS the CU-specific logic.",
    comparison: [
      { label: "Adding CU #101 requires:", bad: "New Azure Function ❌\nNew infrastructure ❌\nCode changes ❌\nRedeployment ❌", good: "1 JSON config file ✅\n1 test file ✅\n1 SQL row ✅" }
    ]
  },
  {
    id: 12,
    section: "Lambda Architecture",
    title: "Lambda Architecture — 3 Layers",
    type: "lambda",
    layers: [
      {
        name: "Raw Layer",
        subtitle: "Store everything as-is forever",
        color: "#64748b",
        icon: "🗃️",
        where: "Azure Blob Storage — /raw/{cu_id}/{date}/",
        what: "Original file, byte for byte, never modified",
        why: "Reprocessing, audit trail, regulatory compliance",
        when: "ALWAYS — copied before any processing begins"
      },
      {
        name: "Batch Layer",
        subtitle: "Complete historical record",
        color: "#6366f1",
        icon: "📚",
        where: "Azure Data Lake",
        what: "Every file, every record, all history partitioned by CU + date",
        why: "Historical analysis, backfilling new columns, full reprocessing",
        when: "Every ingestion run — full file written"
      },
      {
        name: "Speed Layer",
        subtitle: "Latest state, fast queries",
        color: "#10b981",
        icon: "⚡",
        where: "Azure SQL Database — Members table",
        what: "Only new + changed records written. Unchanged records skipped.",
        why: "Applications, APIs, BI tools query here. Always current, always fast.",
        when: "Only new/changed rows — detected via record_hash"
      }
    ],
    changeDetection: [
      { type: "New member", hash: "Hash not in last month", action: "INSERT into SQL", color: "#10b981" },
      { type: "Changed member", hash: "Hash different from last month", action: "UPDATE in SQL", color: "#f59e0b" },
      { type: "Unchanged", hash: "Hash matches last month", action: "Skip — no write", color: "#64748b" }
    ]
  },
  {
    id: 13,
    section: "Lambda Architecture",
    title: "Why Raw Archive Matters",
    type: "raw-archive",
    scenarios: [
      {
        title: "Scenario 1: Mapping Config Had a Bug",
        icon: "🐛",
        color: "#ef4444",
        without: "Bug discovered after processing 5000 records. Original file deleted. Must call CU to resend. Relationship damaged. Delays.",
        with: "Fix the config. Reprocess from /raw/cu_gamma/2026-04-01/. Done in minutes. CU never knows."
      },
      {
        title: "Scenario 2: Regulatory Audit",
        icon: "📋",
        color: "#f59e0b",
        without: "Regulator asks for original March 2025 file. Only transformed SQL data exists. Can't prove original content. Compliance risk.",
        with: "Pull exact original file from /raw/lakeside-cu/2025-03-01/. Byte-for-byte original. Full audit trail."
      },
      {
        title: "Scenario 3: New Column Added to Schema",
        icon: "➕",
        color: "#6366f1",
        without: "New column added 6 months later. Historical data is transformed SQL only. Can't backfill from original files.",
        with: "All original files in /raw/. Update config to map new column. Reprocess all history. SQL backfilled."
      }
    ]
  },
  {
    id: 14,
    section: "Eventing",
    title: "How Eventing Works",
    type: "eventing-why",
    without: {
      title: "Without Eventing — Tight Coupling",
      color: "#ef4444",
      problems: ["AdaptorEngine directly calls Dashboard API", "AdaptorEngine directly calls Alert System", "AdaptorEngine directly calls Audit Service", "If Dashboard is down → ingestion fails", "100 CUs = 300 simultaneous direct calls → overload", "Adding new subscriber = change AdaptorEngine code"]
    },
    with: {
      title: "With Eventing — Loose Coupling",
      color: "#10b981",
      benefits: ["AdaptorEngine fires ONE event and moves on", "Dashboard subscribes independently", "Alert System subscribes independently", "If Dashboard is down → messages wait in queue safely", "100 CUs = 100 messages buffered and delivered at own pace", "Adding new subscriber = zero changes to AdaptorEngine"]
    }
  },
  {
    id: 15,
    section: "Eventing",
    title: "Service Bus — 5 Topics",
    type: "topics",
    topics: [
      { name: "file-arrived", publisher: "Event Grid (blob trigger)", subscribers: ["FileRouterFunction"], payload: '{ cu_id, file_path, file_size, arrived_at, mode }', color: "#6366f1", when: "Moment file lands in blob" },
      { name: "ingestion-ready", publisher: "FileRouterFunction", subscribers: ["AdaptorEngineFunction"], payload: '{ cu_id, file_path, config_path, file_format }', color: "#0ea5e9", when: "After config loaded + validated" },
      { name: "ingestion-started", publisher: "AdaptorEngineFunction", subscribers: ["Dashboard", "Audit Service"], payload: '{ cu_id, job_id, total_rows, started_at }', color: "#f59e0b", when: "Before first row processed" },
      { name: "ingestion-completed", publisher: "AdaptorEngineFunction", subscribers: ["Dashboard", "Audit Service", "BI System", "CU Liaison"], payload: '{ cu_id, job_id, total_rows, success, failed, new, updated, unchanged, sla_met }', color: "#10b981", when: "All rows processed successfully" },
      { name: "ingestion-failed", publisher: "AdaptorEngineFunction", subscribers: ["Alert System", "Retry Service", "Audit Service"], payload: '{ cu_id, job_id, error, failed_at_row, timestamp }', color: "#ef4444", when: "Any unrecoverable error" }
    ]
  },
  {
    id: 16,
    section: "Eventing",
    title: "Eventing at 100 CUs — Monday 9am",
    type: "batch-eventing",
    steps: [
      { time: "9:00am", event: "100 CUs drop files simultaneously", icon: "📁", color: "#6366f1" },
      { time: "9:00am", event: "100 file-arrived events → Service Bus queue", icon: "📬", color: "#0ea5e9" },
      { time: "9:01am", event: "Batch 1: CU_001–010 picked up (maxConcurrentCalls: 10)", icon: "⚙️", color: "#f59e0b" },
      { time: "9:11am", event: "Batch 2: CU_011–020 picked up automatically", icon: "⚙️", color: "#f59e0b" },
      { time: "10:30am", event: "All 100 CUs processed. 100 ingestion-completed events fired.", icon: "✅", color: "#10b981" },
      { time: "10:30am", event: "Dashboard refreshed. Audit logs written. BI cache invalidated.", icon: "📊", color: "#8b5cf6" }
    ],
    note: "Service Bus acts as the safety buffer — no system overwhelms another. Each subscriber processes at its own pace."
  },
  {
    id: 17,
    section: "Scale",
    title: "Scaling to 100+ CUs",
    type: "scale",
    fixed: ["2 Azure Functions (FileRouter + AdaptorEngine)", "5 Service Bus Topics", "1 Target SQL Schema", "1 Shared C# Engine (TrueStage.Core)", "1 CLAUDE.md + skill files", "Azure Infrastructure (blob, event grid, key vault)"],
    grows: ["1 JSON config per CU in /configs/", "1 integration test file per CU", "1 row in CU_Registry SQL table", "1 entry in Claude Code memory file"],
    adding: "Adding CU #101 = 1 JSON config + 1 test file. Zero infrastructure changes. Zero code changes. Zero redeployment."
  },
  {
    id: 18,
    section: "Scale",
    title: "Claude Code Setup",
    type: "claude-code",
    command: "Onboard the new CU CSV files in ./incoming/cu_gamma/",
    flow: [
      { step: "CLAUDE.md loaded", detail: "Rules, conventions, schema path, tech stack", icon: "📋" },
      { step: "Memory files read", detail: "Past CU patterns, known issues", icon: "🧠" },
      { step: "Agent Team runs", detail: "File analyzer + schema reader + pattern matcher in parallel", icon: "🤖" },
      { step: "Mapping shown", detail: "Human confirms the mapping table", icon: "👤" },
      { step: "Config generated", detail: "cu_gamma_mapping.json written", icon: "📝" },
      { step: "Tests generated", detail: "CUGammaTests.cs written", icon: "🧪" },
      { step: "Tests run + fixed", detail: "dotnet test → auto-fix loop", icon: "🔧" },
      { step: "PR opened", detail: "feature/onboard-cu-gamma committed", icon: "🚀" }
    ]
  },
  {
    id: 19,
    section: "Implementation",
    title: "Implementation Phases",
    type: "phases",
    phases: [
      { week: "Week 1–2", phase: "Phase 1", title: "Build Shared C# Engine", color: "#6366f1", items: ["TrueStage.Core — interfaces + transformers + readers", "TrueStage.Engine — AdaptorEngine orchestrator", "TrueStage.Function — Azure Functions", "All tests in /tests/ separate from /src/"] },
      { week: "Week 3", phase: "Phase 2–3", title: "Schema + Infrastructure", color: "#0ea5e9", items: ["Create target SQL schema (Members, Ingestion_Log etc.)", "Provision Azure via Terraform (blob, service bus, event grid)", "Azure Key Vault for all secrets — 12 Factor compliant"] },
      { week: "Week 4", phase: "Phase 4", title: "Claude Code Setup", color: "#f59e0b", items: ["Write CLAUDE.md master instructions", "Write 11 skill files (.claude/skills/)", "Write slash commands (.claude/commands/)", "Set up memory files"] },
      { week: "Week 5", phase: "Phase 5–6", title: "First CU Onboarding", color: "#10b981", items: ["Onboard first CU manually as reference pattern", "Validate full runtime flow end to end", "Test all 5 eventing topics", "Regression suite baseline established"] },
      { week: "Week 7+", phase: "Phase 7", title: "Scale", color: "#ef4444", items: ["Each new CU = 1 Claude Code command + PR review", "Regression runs after every onboarding", "Monitor via Application Insights dashboard", "Add CUs indefinitely — zero infrastructure changes"] }
    ]
  },
  {
    id: 20,
    section: "Summary",
    title: "Key Principles Applied",
    type: "principles",
    principles: [
      { name: "SOLID", icon: "🏗️", color: "#6366f1", where: "TrueStage.Core — one responsibility per class, all dependencies injected via interfaces. TransformerFactory open for extension." },
      { name: "12 Factor App", icon: "🔒", color: "#0ea5e9", where: "All secrets from Azure Key Vault. Stateless functions. Structured JSON logs to Application Insights." },
      { name: "Unit Tests", icon: "✅", color: "#10b981", where: "/tests/ folder always separate from /src/. Generated per CU by Claude Code. Mocked external dependencies." },
      { name: "Regression Testing", icon: "🔁", color: "#f59e0b", where: "RegressionSuite runs all CU tests after any change. Onboarding CU #101 cannot break CU #001." },
      { name: "Eventing", icon: "📡", color: "#ef4444", where: "5 Service Bus topics. AdaptorEngine fires and forgets. Subscribers react independently. Loose coupling at scale." },
      { name: "Lambda Architecture", icon: "📚", color: "#8b5cf6", where: "Raw layer (never deleted) → Batch layer (full history) → Speed layer (latest state only). Always reprocessable." },
      { name: "Reusable Components", icon: "♻️", color: "#06b6d4", where: "One shared C# engine. One set of Azure Functions. Config drives all CU differences. Zero copy-paste." }
    ]
  }
];

const colors = {
  bg: "#0f172a",
  card: "#1e293b",
  card2: "#273548",
  border: "#334155",
  text: "#f1f5f9",
  muted: "#94a3b8",
  accent: "#6366f1"
};

export default function App() {
  const [current, setCurrent] = useState(0);
  const slide = slides[current];

  const sections = [...new Set(slides.map(s => s.section))];

  const go = (dir) => {
    setCurrent(c => Math.max(0, Math.min(slides.length - 1, c + dir)));
  };

  return (
    <div style={{ background: colors.bg, minHeight: "100vh", fontFamily: "'Inter', sans-serif", color: colors.text, display: "flex", flexDirection: "column" }}>
      {/* Top nav */}
      <div style={{ background: colors.card, borderBottom: `1px solid ${colors.border}`, padding: "10px 24px", display: "flex", gap: 8, flexWrap: "wrap", alignItems: "center" }}>
        {sections.map(s => (
          <button key={s} onClick={() => setCurrent(slides.findIndex(sl => sl.section === s))}
            style={{ background: slide.section === s ? colors.accent : "transparent", color: slide.section === s ? "#fff" : colors.muted, border: `1px solid ${slide.section === s ? colors.accent : colors.border}`, borderRadius: 6, padding: "4px 12px", fontSize: 12, cursor: "pointer" }}>
            {s}
          </button>
        ))}
        <span style={{ marginLeft: "auto", color: colors.muted, fontSize: 12 }}>{current + 1} / {slides.length}</span>
      </div>

      {/* Slide */}
      <div style={{ flex: 1, padding: "24px 32px", overflowY: "auto" }}>
        <SlideRenderer slide={slide} />
      </div>

      {/* Controls */}
      <div style={{ background: colors.card, borderTop: `1px solid ${colors.border}`, padding: "12px 32px", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
        <button onClick={() => go(-1)} disabled={current === 0}
          style={{ background: current === 0 ? colors.border : colors.accent, color: "#fff", border: "none", borderRadius: 8, padding: "8px 20px", cursor: current === 0 ? "default" : "pointer", opacity: current === 0 ? 0.4 : 1 }}>
          ← Prev
        </button>
        <div style={{ display: "flex", gap: 4 }}>
          {slides.map((_, i) => (
            <div key={i} onClick={() => setCurrent(i)}
              style={{ width: i === current ? 20 : 6, height: 6, borderRadius: 3, background: i === current ? colors.accent : colors.border, cursor: "pointer", transition: "all 0.2s" }} />
          ))}
        </div>
        <button onClick={() => go(1)} disabled={current === slides.length - 1}
          style={{ background: current === slides.length - 1 ? colors.border : colors.accent, color: "#fff", border: "none", borderRadius: 8, padding: "8px 20px", cursor: current === slides.length - 1 ? "default" : "pointer", opacity: current === slides.length - 1 ? 0.4 : 1 }}>
          Next →
        </button>
      </div>
    </div>
  );
}

function SlideRenderer({ slide }) {
  const c = colors;

  if (slide.type === "cover") return (
    <div style={{ display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center", minHeight: "60vh", textAlign: "center" }}>
      <div style={{ background: "linear-gradient(135deg, #6366f1, #10b981)", borderRadius: 20, padding: "40px 60px", marginBottom: 32 }}>
        <div style={{ fontSize: 48, marginBottom: 16 }}>🏦</div>
        <h1 style={{ fontSize: 36, fontWeight: 800, margin: 0 }}>{slide.title}</h1>
        <p style={{ fontSize: 18, opacity: 0.85, marginTop: 8 }}>{slide.subtitle}</p>
      </div>
      <p style={{ color: c.muted, fontSize: 14 }}>Credit Union Data Integration Platform — Architecture & Flow</p>
    </div>
  );

  if (slide.type === "problem") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 24 }}>{slide.title}</h2>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 16 }}>
        {[
          { label: "The Problem", value: slide.content.problem, color: "#ef4444", icon: "⚠️" },
          { label: "The Goal", value: slide.content.goal, color: "#10b981", icon: "🎯" },
          { label: "The Approach", value: slide.content.approach, color: "#6366f1", icon: "💡" }
        ].map(item => (
          <div key={item.label} style={{ background: c.card, border: `1px solid ${item.color}40`, borderRadius: 12, padding: 24 }}>
            <div style={{ fontSize: 32, marginBottom: 12 }}>{item.icon}</div>
            <div style={{ color: item.color, fontWeight: 700, fontSize: 14, marginBottom: 8 }}>{item.label}</div>
            <p style={{ color: c.muted, fontSize: 14, lineHeight: 1.6, margin: 0 }}>{item.value}</p>
          </div>
        ))}
      </div>
    </div>
  );

  if (slide.type === "two-modes") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 24 }}>{slide.title}</h2>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 20 }}>
        {slide.modes.map(m => (
          <div key={m.title} style={{ background: c.card, border: `2px solid ${m.color}`, borderRadius: 12, padding: 24 }}>
            <div style={{ fontSize: 40, marginBottom: 8 }}>{m.icon}</div>
            <h3 style={{ color: m.color, fontSize: 22, fontWeight: 700, margin: "0 0 4px" }}>{m.title}</h3>
            <p style={{ color: c.muted, fontSize: 13, margin: "0 0 16px" }}>{m.subtitle}</p>
            {m.items.map((item, i) => (
              <div key={i} style={{ display: "flex", alignItems: "center", gap: 8, marginBottom: 8 }}>
                <div style={{ width: 20, height: 20, borderRadius: "50%", background: m.color, color: "#fff", fontSize: 11, display: "flex", alignItems: "center", justifyContent: "center", flexShrink: 0 }}>{i + 1}</div>
                <span style={{ fontSize: 13, color: c.text }}>{item}</span>
              </div>
            ))}
          </div>
        ))}
      </div>
    </div>
  );

  if (slide.type === "flow") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 24 }}>{slide.title}</h2>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16 }}>
        {slide.steps.map((s, i) => (
          <div key={i} style={{ background: c.card, border: `1px solid ${s.color}40`, borderRadius: 12, padding: 20 }}>
            <div style={{ display: "flex", alignItems: "center", gap: 10, marginBottom: 12 }}>
              <div style={{ background: s.color, borderRadius: 6, padding: "4px 10px", fontSize: 11, fontWeight: 700 }}>{s.phase}</div>
              <h3 style={{ margin: 0, fontSize: 16, fontWeight: 700, color: s.color }}>{s.title}</h3>
            </div>
            {s.items.map((item, j) => (
              <div key={j} style={{ display: "flex", gap: 8, marginBottom: 6 }}>
                <span style={{ color: s.color, flexShrink: 0 }}>→</span>
                <span style={{ fontSize: 13, color: c.muted }}>{item}</span>
              </div>
            ))}
          </div>
        ))}
      </div>
    </div>
  );

  if (slide.type === "three-players") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 24 }}>{slide.title}</h2>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 16 }}>
        {slide.players.map(p => (
          <div key={p.name} style={{ background: c.card, border: `2px solid ${p.color}`, borderRadius: 12, padding: 20 }}>
            <div style={{ fontSize: 32, marginBottom: 8 }}>{p.icon}</div>
            <h3 style={{ color: p.color, fontSize: 18, fontWeight: 700, margin: "0 0 4px" }}>{p.name}</h3>
            <p style={{ color: c.muted, fontSize: 12, margin: "0 0 16px" }}>{p.role}</p>
            <div style={{ fontSize: 12, color: "#10b981", fontWeight: 600, marginBottom: 6 }}>DOES:</div>
            {p.does.map((d, i) => <div key={i} style={{ fontSize: 12, color: c.muted, marginBottom: 4 }}>✅ {d}</div>)}
            <div style={{ fontSize: 12, color: "#ef4444", fontWeight: 600, margin: "12px 0 6px" }}>DOES NOT:</div>
            {p.doesnt.map((d, i) => <div key={i} style={{ fontSize: 12, color: c.muted, marginBottom: 4 }}>❌ {d}</div>)}
          </div>
        ))}
      </div>
    </div>
  );

  if (slide.type === "mapping-table") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 8 }}>{slide.title}</h2>
      <p style={{ color: c.muted, fontSize: 13, marginBottom: 20 }}>🟢 HIGH = auto-approved &nbsp;|&nbsp; 🟡 MEDIUM = human must confirm &nbsp;|&nbsp; 🔴 LOW = human must manually map</p>
      <div style={{ background: c.card, borderRadius: 12, overflow: "hidden", border: `1px solid ${c.border}` }}>
        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 120px 2fr", background: c.card2, padding: "12px 20px", fontSize: 12, fontWeight: 700, color: c.muted }}>
          <span>SOURCE COLUMN</span><span>TARGET COLUMN</span><span>CONFIDENCE</span><span>TRANSFORM</span>
        </div>
        {slide.rows.map((row, i) => (
          <div key={i} style={{ display: "grid", gridTemplateColumns: "1fr 1fr 120px 2fr", padding: "12px 20px", borderTop: `1px solid ${c.border}`, background: i % 2 === 0 ? "transparent" : "#1a2535", alignItems: "center" }}>
            <code style={{ fontSize: 13, color: "#fbbf24" }}>{row.source}</code>
            <code style={{ fontSize: 13, color: "#60a5fa" }}>{row.target}</code>
            <span style={{ fontSize: 12, fontWeight: 600, color: row.status === "green" ? "#10b981" : row.status === "yellow" ? "#f59e0b" : "#ef4444" }}>
              {row.status === "green" ? "🟢" : row.status === "yellow" ? "🟡" : "🔴"} {row.confidence}
            </span>
            <span style={{ fontSize: 12, color: c.muted }}>{row.transform}</span>
          </div>
        ))}
      </div>
    </div>
  );

  if (slide.type === "adaptor") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 12 }}>{slide.title}</h2>
      <div style={{ background: `${c.accent}20`, border: `1px solid ${c.accent}`, borderRadius: 10, padding: 16, marginBottom: 24 }}>
        <p style={{ margin: 0, fontSize: 15, color: c.text, fontStyle: "italic" }}>💡 {slide.analogy}</p>
      </div>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 16 }}>
        {slide.components.map(comp => (
          <div key={comp.title} style={{ background: c.card, border: `2px solid ${comp.color}`, borderRadius: 12, padding: 20 }}>
            <div style={{ fontSize: 32, marginBottom: 8 }}>{comp.icon}</div>
            <h3 style={{ color: comp.color, fontSize: 14, fontWeight: 700, margin: "0 0 4px" }}>{comp.title}</h3>
            <p style={{ color: c.muted, fontSize: 12, margin: "0 0 16px" }}>{comp.subtitle}</p>
            {comp.items.map((item, i) => <div key={i} style={{ fontSize: 12, color: c.muted, marginBottom: 6 }}>→ {item}</div>)}
          </div>
        ))}
      </div>
    </div>
  );

  if (slide.type === "runtime-flow") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 20 }}>{slide.title}</h2>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
        {slide.steps.map((s, i) => (
          <div key={i} style={{ background: c.card, border: `1px solid ${s.color}40`, borderRadius: 10, padding: 16, display: "flex", gap: 12, alignItems: "flex-start" }}>
            <div style={{ background: s.color, borderRadius: 8, width: 36, height: 36, display: "flex", alignItems: "center", justifyContent: "center", fontSize: 18, flexShrink: 0 }}>{s.icon}</div>
            <div>
              <div style={{ fontWeight: 700, fontSize: 14, color: s.color, marginBottom: 4 }}>{i + 1}. {s.title}</div>
              <div style={{ fontSize: 12, color: c.muted }}>{s.desc}</div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );

  if (slide.type === "drift") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 20 }}>{slide.title}</h2>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16 }}>
        {slide.types.map((d, i) => (
          <div key={i} style={{ background: c.card, border: `2px solid ${d.color}`, borderRadius: 12, padding: 20 }}>
            <div style={{ display: "flex", alignItems: "center", gap: 10, marginBottom: 12 }}>
              <span style={{ fontSize: 24 }}>{d.icon}</span>
              <h3 style={{ margin: 0, color: d.color, fontSize: 16 }}>{d.type}</h3>
            </div>
            <div style={{ background: "#0f172a", borderRadius: 8, padding: 12, marginBottom: 12 }}>
              <pre style={{ margin: 0, fontSize: 11, color: "#94a3b8", whiteSpace: "pre-wrap" }}>{d.example}</pre>
            </div>
            <div style={{ background: `${d.color}20`, borderRadius: 6, padding: "8px 12px", marginBottom: 8 }}>
              <span style={{ color: d.color, fontWeight: 700, fontSize: 13 }}>{d.action}</span>
            </div>
            <p style={{ fontSize: 12, color: c.muted, margin: 0 }}>{d.detail}</p>
          </div>
        ))}
      </div>
    </div>
  );

  if (slide.type === "functions") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 8 }}>{slide.title}</h2>
      <div style={{ background: "#10b98120", border: "1px solid #10b981", borderRadius: 8, padding: 12, marginBottom: 20 }}>
        <p style={{ margin: 0, fontSize: 14, color: "#10b981", fontWeight: 600 }}>💡 {slide.insight}</p>
      </div>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 16 }}>
        {slide.functions.map(f => (
          <div key={f.name} style={{ background: c.card, border: `2px solid ${f.color}`, borderRadius: 12, padding: 20 }}>
            <h3 style={{ color: f.color, fontSize: 15, fontWeight: 700, margin: "0 0 6px" }}>{f.name}</h3>
            <div style={{ background: `${f.color}20`, borderRadius: 6, padding: "4px 10px", fontSize: 11, color: f.color, marginBottom: 14, display: "inline-block" }}>Trigger: {f.trigger}</div>
            {f.responsibilities.map((r, i) => <div key={i} style={{ fontSize: 12, color: c.muted, marginBottom: 6 }}>→ {r}</div>)}
          </div>
        ))}
      </div>
    </div>
  );

  if (slide.type === "one-function") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 20 }}>{slide.title}</h2>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 20 }}>
        <div>
          <div style={{ background: "#0f172a", borderRadius: 12, padding: 20, marginBottom: 16 }}>
            {slide.code.map((c2, i) => (
              <div key={i} style={{ marginBottom: 12 }}>
                <div style={{ fontSize: 11, color: colors.muted, marginBottom: 4 }}>{c2.label}</div>
                <code style={{ fontSize: 12, color: "#fbbf24", background: "#1e293b", padding: "4px 8px", borderRadius: 4, display: "block" }}>{c2.value}</code>
              </div>
            ))}
          </div>
          <div style={{ background: `${colors.accent}20`, border: `1px solid ${colors.accent}`, borderRadius: 8, padding: 14 }}>
            <p style={{ margin: 0, fontSize: 13, color: colors.text, fontStyle: "italic" }}>💡 {slide.principle}</p>
          </div>
        </div>
        <div>
          <h3 style={{ color: colors.muted, fontSize: 14, fontWeight: 600, marginBottom: 16 }}>ADDING A NEW CU REQUIRES:</h3>
          <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
            <div style={{ background: "#ef444420", border: "1px solid #ef4444", borderRadius: 10, padding: 16 }}>
              <div style={{ color: "#ef4444", fontWeight: 700, fontSize: 13, marginBottom: 10 }}>❌ WHAT YOU MIGHT THINK</div>
              {["New Azure Function", "New infrastructure", "Code changes", "Redeployment"].map((item, i) => (
                <div key={i} style={{ fontSize: 12, color: colors.muted, marginBottom: 6 }}>❌ {item}</div>
              ))}
            </div>
            <div style={{ background: "#10b98120", border: "1px solid #10b981", borderRadius: 10, padding: 16 }}>
              <div style={{ color: "#10b981", fontWeight: 700, fontSize: 13, marginBottom: 10 }}>✅ REALITY</div>
              {["1 JSON config file", "1 test file", "1 SQL row in CU_Registry"].map((item, i) => (
                <div key={i} style={{ fontSize: 12, color: colors.muted, marginBottom: 6 }}>✅ {item}</div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );

  if (slide.type === "lambda") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 20 }}>{slide.title}</h2>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 16, marginBottom: 20 }}>
        {slide.layers.map((l, i) => (
          <div key={i} style={{ background: c.card, border: `2px solid ${l.color}`, borderRadius: 12, padding: 20 }}>
            <div style={{ fontSize: 32, marginBottom: 8 }}>{l.icon}</div>
            <h3 style={{ color: l.color, fontSize: 18, fontWeight: 700, margin: "0 0 4px" }}>{l.name}</h3>
            <p style={{ color: c.muted, fontSize: 12, margin: "0 0 16px" }}>{l.subtitle}</p>
            <div style={{ fontSize: 12, marginBottom: 8 }}><span style={{ color: l.color, fontWeight: 600 }}>WHERE: </span><span style={{ color: c.muted }}>{l.where}</span></div>
            <div style={{ fontSize: 12, marginBottom: 8 }}><span style={{ color: l.color, fontWeight: 600 }}>WHAT: </span><span style={{ color: c.muted }}>{l.what}</span></div>
            <div style={{ fontSize: 12, marginBottom: 8 }}><span style={{ color: l.color, fontWeight: 600 }}>WHY: </span><span style={{ color: c.muted }}>{l.why}</span></div>
            <div style={{ fontSize: 12 }}><span style={{ color: l.color, fontWeight: 600 }}>WHEN: </span><span style={{ color: c.muted }}>{l.when}</span></div>
          </div>
        ))}
      </div>
      <div>
        <h3 style={{ fontSize: 15, fontWeight: 700, marginBottom: 12, color: c.muted }}>CHANGE DETECTION VIA record_hash</h3>
        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 12 }}>
          {slide.changeDetection.map((cd, i) => (
            <div key={i} style={{ background: c.card, border: `1px solid ${cd.color}`, borderRadius: 8, padding: 14 }}>
              <div style={{ fontWeight: 700, color: cd.color, fontSize: 13, marginBottom: 6 }}>{cd.type}</div>
              <div style={{ fontSize: 11, color: c.muted, marginBottom: 8 }}>{cd.hash}</div>
              <div style={{ background: `${cd.color}20`, borderRadius: 4, padding: "4px 8px", fontSize: 12, color: cd.color }}>{cd.action}</div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );

  if (slide.type === "raw-archive") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 20 }}>{slide.title}</h2>
      <div style={{ display: "flex", flexDirection: "column", gap: 16 }}>
        {slide.scenarios.map((s, i) => (
          <div key={i} style={{ background: c.card, border: `1px solid ${s.color}40`, borderRadius: 12, padding: 20 }}>
            <div style={{ display: "flex", alignItems: "center", gap: 10, marginBottom: 14 }}>
              <span style={{ fontSize: 24 }}>{s.icon}</span>
              <h3 style={{ margin: 0, fontSize: 16, fontWeight: 700, color: s.color }}>{s.title}</h3>
            </div>
            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
              <div style={{ background: "#ef444415", border: "1px solid #ef444440", borderRadius: 8, padding: 14 }}>
                <div style={{ color: "#ef4444", fontWeight: 700, fontSize: 12, marginBottom: 8 }}>❌ WITHOUT RAW ARCHIVE</div>
                <p style={{ fontSize: 12, color: c.muted, margin: 0 }}>{s.without}</p>
              </div>
              <div style={{ background: "#10b98115", border: "1px solid #10b98140", borderRadius: 8, padding: 14 }}>
                <div style={{ color: "#10b981", fontWeight: 700, fontSize: 12, marginBottom: 8 }}>✅ WITH RAW ARCHIVE</div>
                <p style={{ fontSize: 12, color: c.muted, margin: 0 }}>{s.with}</p>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );

  if (slide.type === "eventing-why") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 20 }}>{slide.title}</h2>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 20 }}>
        {[slide.without, slide.with].map((panel, i) => (
          <div key={i} style={{ background: c.card, border: `2px solid ${panel.color}`, borderRadius: 12, padding: 24 }}>
            <h3 style={{ color: panel.color, fontSize: 16, fontWeight: 700, margin: "0 0 16px" }}>{panel.title}</h3>
            {(panel.problems || panel.benefits).map((item, j) => (
              <div key={j} style={{ display: "flex", gap: 8, marginBottom: 10 }}>
                <span style={{ color: panel.color, flexShrink: 0 }}>{i === 0 ? "❌" : "✅"}</span>
                <span style={{ fontSize: 13, color: c.muted }}>{item}</span>
              </div>
            ))}
          </div>
        ))}
      </div>
    </div>
  );

  if (slide.type === "topics") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 16 }}>{slide.title}</h2>
      <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
        {slide.topics.map((t, i) => (
          <div key={i} style={{ background: c.card, border: `1px solid ${t.color}`, borderRadius: 10, padding: 16, display: "grid", gridTemplateColumns: "180px 1fr 1fr 1fr", gap: 12, alignItems: "center" }}>
            <div style={{ background: `${t.color}20`, borderRadius: 6, padding: "6px 12px" }}>
              <code style={{ color: t.color, fontSize: 12, fontWeight: 700 }}>{t.name}</code>
            </div>
            <div><span style={{ fontSize: 10, color: c.muted, display: "block", marginBottom: 2 }}>PUBLISHER</span><span style={{ fontSize: 12, color: c.text }}>{t.publisher}</span></div>
            <div><span style={{ fontSize: 10, color: c.muted, display: "block", marginBottom: 2 }}>SUBSCRIBERS</span>{t.subscribers.map((s, j) => <span key={j} style={{ fontSize: 12, color: c.text, display: "block" }}>{s}</span>)}</div>
            <div><span style={{ fontSize: 10, color: c.muted, display: "block", marginBottom: 2 }}>WHEN</span><span style={{ fontSize: 12, color: c.text }}>{t.when}</span></div>
          </div>
        ))}
      </div>
    </div>
  );

  if (slide.type === "batch-eventing") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 20 }}>{slide.title}</h2>
      <div style={{ display: "flex", flexDirection: "column", gap: 10, marginBottom: 20 }}>
        {slide.steps.map((s, i) => (
          <div key={i} style={{ background: c.card, border: `1px solid ${s.color}40`, borderRadius: 10, padding: 14, display: "flex", gap: 16, alignItems: "center" }}>
            <div style={{ background: s.color, borderRadius: 6, padding: "4px 10px", fontSize: 12, fontWeight: 700, color: "#fff", minWidth: 70, textAlign: "center" }}>{s.time}</div>
            <span style={{ fontSize: 20 }}>{s.icon}</span>
            <span style={{ fontSize: 13, color: c.text }}>{s.event}</span>
          </div>
        ))}
      </div>
      <div style={{ background: "#6366f120", border: "1px solid #6366f1", borderRadius: 10, padding: 16 }}>
        <p style={{ margin: 0, fontSize: 13, color: c.text }}>💡 {slide.note}</p>
      </div>
    </div>
  );

  if (slide.type === "scale") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 20 }}>{slide.title}</h2>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 20, marginBottom: 20 }}>
        <div style={{ background: c.card, border: "1px solid #10b981", borderRadius: 12, padding: 20 }}>
          <h3 style={{ color: "#10b981", fontSize: 15, fontWeight: 700, margin: "0 0 14px" }}>BUILT ONCE — NEVER CHANGES</h3>
          {slide.fixed.map((item, i) => <div key={i} style={{ fontSize: 13, color: c.muted, marginBottom: 8 }}>♾️ {item}</div>)}
        </div>
        <div style={{ background: c.card, border: "1px solid #6366f1", borderRadius: 12, padding: 20 }}>
          <h3 style={{ color: "#6366f1", fontSize: 15, fontWeight: 700, margin: "0 0 14px" }}>GROWS WITH EACH NEW CU</h3>
          {slide.grows.map((item, i) => <div key={i} style={{ fontSize: 13, color: c.muted, marginBottom: 8 }}>➕ {item}</div>)}
        </div>
      </div>
      <div style={{ background: "#10b98120", border: "1px solid #10b981", borderRadius: 10, padding: 16 }}>
        <p style={{ margin: 0, fontSize: 15, color: "#10b981", fontWeight: 600 }}>🎯 {slide.adding}</p>
      </div>
    </div>
  );

  if (slide.type === "claude-code") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 16 }}>{slide.title}</h2>
      <div style={{ background: "#0f172a", border: "1px solid #6366f1", borderRadius: 10, padding: 16, marginBottom: 20 }}>
        <div style={{ fontSize: 11, color: c.muted, marginBottom: 6 }}>YOU TYPE IN CLAUDE CODE:</div>
        <code style={{ fontSize: 15, color: "#a78bfa" }}>"{slide.command}"</code>
      </div>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
        {slide.flow.map((f, i) => (
          <div key={i} style={{ background: c.card, border: `1px solid ${c.border}`, borderRadius: 10, padding: 14, display: "flex", gap: 12, alignItems: "flex-start" }}>
            <div style={{ background: c.accent, borderRadius: 6, width: 28, height: 28, display: "flex", alignItems: "center", justifyContent: "center", fontSize: 14, flexShrink: 0 }}>{f.icon}</div>
            <div>
              <div style={{ fontWeight: 700, fontSize: 13, color: c.text, marginBottom: 4 }}>{i + 1}. {f.step}</div>
              <div style={{ fontSize: 12, color: c.muted }}>{f.detail}</div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );

  if (slide.type === "phases") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 20 }}>{slide.title}</h2>
      <div style={{ display: "flex", flexDirection: "column", gap: 12 }}>
        {slide.phases.map((p, i) => (
          <div key={i} style={{ background: c.card, border: `1px solid ${p.color}40`, borderRadius: 10, padding: 16, display: "grid", gridTemplateColumns: "100px 120px 1fr", gap: 16, alignItems: "start" }}>
            <div style={{ background: `${p.color}20`, borderRadius: 6, padding: "4px 10px", textAlign: "center" }}>
              <div style={{ color: p.color, fontSize: 11, fontWeight: 700 }}>{p.week}</div>
            </div>
            <div>
              <div style={{ color: p.color, fontWeight: 700, fontSize: 13 }}>{p.phase}</div>
              <div style={{ color: c.text, fontSize: 13, fontWeight: 600 }}>{p.title}</div>
            </div>
            <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
              {p.items.map((item, j) => (
                <div key={j} style={{ background: c.card2, border: `1px solid ${c.border}`, borderRadius: 6, padding: "4px 10px", fontSize: 11, color: c.muted }}>→ {item}</div>
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );

  if (slide.type === "principles") return (
    <div>
      <h2 style={{ fontSize: 28, fontWeight: 700, marginBottom: 20 }}>{slide.title}</h2>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 14 }}>
        {slide.principles.map((p, i) => (
          <div key={i} style={{ background: c.card, border: `1px solid ${p.color}40`, borderRadius: 10, padding: 16, display: "flex", gap: 12 }}>
            <div style={{ background: `${p.color}20`, borderRadius: 8, width: 40, height: 40, display: "flex", alignItems: "center", justifyContent: "center", fontSize: 20, flexShrink: 0 }}>{p.icon}</div>
            <div>
              <div style={{ fontWeight: 700, color: p.color, fontSize: 14, marginBottom: 6 }}>{p.name}</div>
              <div style={{ fontSize: 12, color: c.muted, lineHeight: 1.5 }}>{p.where}</div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );

  return <div style={{ color: c.muted }}>Slide type not found: {slide.type}</div>;
}