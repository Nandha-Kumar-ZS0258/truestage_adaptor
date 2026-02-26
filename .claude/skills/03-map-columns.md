# Skill: Map Source Columns to Target

Given:
- Source column list (from skill 02)
- Target schema (`/schema/target.sql`)
- Past patterns (`/.claude/memory/onboarded_cus.md`)

Map each source column to a target column with confidence score.

## Confidence Rules
- **HIGH 🟢** — exact match OR known abbreviation (FName→first_name, DOB→date_of_birth, MemberID→source_member_id)
- **MEDIUM 🟡** — partial semantic match (Bal→account_balance, Status→member_status)
- **LOW 🔴** — no clear match — ask human

## Unmapped columns
Any source column that has no clear target → list separately as "will be ignored".

## Output
```
Mapping Proposal for CU_GAMMA
══════════════════════════════════════════════════════════════════════════
 Source Column   Target Column      Confidence  Proposed Transform
──────────────── ─────────────────  ──────────  ─────────────────────────
 mem_no          source_member_id   HIGH  🟢    trim
 f_name          first_name         HIGH  🟢    trim
 l_name          last_name          HIGH  🟢    trim
 dob             date_of_birth      HIGH  🟢    date:MM/dd/yyyy
 acct_bal        account_balance    MEDIUM 🟡   trim, to_decimal
 status          member_status      MEDIUM 🟡   value_map:A->ACTIVE,I->INACTIVE
 branch_cd       branch_code        LOW  🔴     trim
 email_addr      email              HIGH  🟢    trim
──────────────── ─────────────────  ──────────  ─────────────────────────
 middle_name     (no match)         IGNORE      —
══════════════════════════════════════════════════════════════════════════

⚠️ Please confirm or correct MEDIUM and LOW rows before I generate the config.
```
