# Onboarded Credit Unions — Pattern Memory

This file is updated every time a new CU is onboarded.
Use patterns here to improve confidence when onboarding similar CUs.

---

## CU_ALPHA — Alpha Community Credit Union
- **Source format:** CSV, comma-delimited
- **Date format:** `MM/dd/yyyy`
- **Member ID column:** `MemberID`
- **Name columns:** `FName`, `LName` (short abbreviations)
- **Balance column:** `Bal` — plain decimal, no $ prefix
- **Status codes:** single character — `A`=ACTIVE, `I`=INACTIVE, `C`=CLOSED
- **Branch column:** `Branch`
- **Onboarded:** 2026-02-26
- **Config:** `/configs/cu_alpha_mapping.json`

---

## CU_BETA — Beta Federal Credit Union
- **Source format:** CSV, pipe-delimited (`|`)
- **Date format:** `yyyy-MM-dd` (ISO 8601)
- **Member ID column:** `MEMBER_NUM` (all caps, underscore-separated)
- **Name columns:** `FIRST_NAME`, `LAST_NAME` (full words, all caps)
- **Balance column:** `ACCT_BAL` — plain decimal
- **Status codes:** full words — `ACTIVE`, `INACTIVE`, `CLOSED`
- **Branch column:** `BRANCH_ID`
- **Onboarded:** 2026-02-26
- **Config:** `/configs/cu_beta_mapping.json`

---

## CU_GAMMA — Coastal Community Credit Union
- **Source format:** CSV, comma-delimited
- **Date format:** `yyyy-MM-dd` (ISO 8601)
- **Member ID column:** `member_id` (lowercase, sequential e.g. CC00000001)
- **Name columns:** `member_name` — COMBINED full name (no separate first/last); mapped to `first_name`
- **Balance column:** not present in source
- **Status column:** not present in source
- **Branch column:** not present in source
- **Extra columns (unmapped):** `entity_uuid`, `street_name`, `building_number`, `postal_code`, `town_name`, `state`, `country`, `tax_id_masked`
- **Phone formats:** mixed — dotted (423.890.2844), dashes (987-938-2337), +1 prefix, parentheses
- **Quirk:** No split transform — full name stored in `first_name`, `last_name` is null
- **Onboarded:** 2026-02-26
- **Config:** `/configs/cu_gamma_mapping.json`

---

## CCCU_MEMBER — Coastal Community Credit Union (alternate column names)
- **Source format:** CSV, comma-delimited
- **Source file:** `CCCU_member_details1.csv`
- **Date format:** `yyyy-MM-dd` (ISO 8601)
- **Member ID column:** `memberid` (no underscore, lowercase)
- **UUID column:** `uuid` (maps to `entity_uuid`)
- **Name column:** `name` (maps to `member_name`, full name combined)
- **DOB column:** `DOB` (uppercase abbreviation)
- **Address columns:** `street` → `street_name`, `building` → `building_number`, `pin` → `postal_code`, `town` → `town_name`
- **Phone column:** `cell` → `phone_number` (cell/mobile)
- **Exact-match columns:** `state`, `country`, `tax_id_masked`, `email`
- **Quirk:** `pin` = postal/zip code (confirmed by user — US zip values like 97562)
- **Onboarded:** 2026-02-26
- **Config:** `/configs/cccu_member_mapping.json`

---

## Patterns Observed

### Column Name Conventions Across CUs
| Concept | CU_ALPHA | CU_BETA | CU_GAMMA | Notes |
|---|---|---|---|---|
| Member ID | `MemberID` | `MEMBER_NUM` | `member_id` | Always map to `source_member_id` |
| First Name | `FName` | `FIRST_NAME` | `member_name` (full) | Abbreviation OR full word OR combined |
| Last Name | `LName` | `LAST_NAME` | _(none)_ | May be absent if combined name field |
| DOB | `DOB` | `BIRTH_DATE` | `birth_date` | Always map to `date_of_birth` |
| Balance | `Bal` | `ACCT_BAL` | _(none)_ | Always to_decimal; check for $ prefix |
| Status | `Status` | `MBR_STATUS` | _(none)_ | Check if single-char codes or full words |

### Date Formats Seen
- `MM/dd/yyyy` — US format (CU_ALPHA)
- `yyyy-MM-dd` — ISO 8601 (CU_BETA)
- `MM-dd-yy` — Short year (not yet seen — watch for it)
- `dd/MM/yyyy` — European format (not yet seen — watch for it)

### Status Code Patterns
- Single char: `A/I/C` → `ACTIVE/INACTIVE/CLOSED`
- Full word: `ACTIVE/INACTIVE/CLOSED` → identity map

### Balance Patterns
- Plain decimal: `1200.50` → `to_decimal`
- With $ prefix: `$1,200.50` → `trim, to_decimal` (strips $ and commas automatically)
