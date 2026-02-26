# Skill: Detect Source Format

Given a file path, detect:
1. **File format** — CSV, JSON, XML, flat-file (fixed-width)
2. **Delimiter** (if CSV) — comma, pipe, tab, semicolon
3. **Has header row** — yes/no
4. **Encoding** — UTF-8, ASCII, etc.
5. **Approximate row count**

## Detection Logic
- `.csv` extension or comma/pipe/tab separated → CSV
- `.json` extension or starts with `{` or `[` → JSON
- `.xml` extension or starts with `<?xml` → XML
- Otherwise → inspect first 5 lines for pattern

## Output
```
Format: CSV
Delimiter: ,
Has header: true
Encoding: UTF-8
Estimated rows: 1024
```
