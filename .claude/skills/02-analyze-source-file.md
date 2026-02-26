# Skill: Analyze Source File

Given a source file (CSV/JSON/XML), analyze and report:

## For each column:
- Column name (exact, case-preserved)
- Detected data type: string | integer | decimal | date | boolean
- Null/empty rate: what % of rows are empty
- Sample values (3 distinct examples)
- If date: probable format (MM/dd/yyyy, yyyy-MM-dd, etc.)
- If decimal: any prefix/suffix ($, %, commas)
- If discrete values: enumerated list (e.g., A, I, C — possible status codes)

## Output format:
```
Column Analysis: cu_alpha_members.csv
Rows: 500

Column          Type      Nulls  Sample Values              Notes
─────────────── ───────── ────── ────────────────────────── ──────────────────────────
MemberID        string    0%     M001, M002, M003           Unique identifier
FName           string    0%     John, Jane, Bob
LName           string    0%     Doe, Smith, Johnson
DOB             date      2%     01/15/1985, 03/22/1990     Format: MM/dd/yyyy
Email           string    5%     john@email.com
Phone           string    3%     555-1234
Bal             decimal   0%     1200.50, 850.00, 0.00      No $ prefix
Status          string    0%     A, I, C                    Discrete: 3 values
Branch          string    0%     BR01, BR02, BR03
```
