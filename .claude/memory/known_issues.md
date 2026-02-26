# Known Issues & Resolutions

---

## Issue: Date parsing fails for ambiguous formats
**Symptom:** `FormatException: Cannot parse date '...' with format '...'`
**Root cause:** Source date format in file doesn't match `date_default_format` in config
**Resolution:** Check first 3-5 sample rows, identify actual format, update `transform` on the `date_of_birth` mapping

---

## Issue: Decimal parsing fails on $-prefixed amounts
**Symptom:** `FormatException: Cannot parse decimal '$1,200.50'`
**Root cause:** `to_decimal` alone doesn't need to strip `$` — `DecimalTransformer` already strips `$` and commas
**Resolution:** Use `"transform": "trim, to_decimal"` — no need for `strip_prefix:$`

---

## Issue: CSV with quoted fields fails
**Symptom:** Column values contain extra quotes or mis-parsed
**Root cause:** CsvHelper `BadDataFound = null` suppresses errors — check for mismatched quotes in source
**Resolution:** Inspect raw file, update `file_delimiter` if needed

---

## Issue: Pipe-delimited file treated as CSV
**Symptom:** All data in single column
**Root cause:** Config `file_delimiter` is `,` but file uses `|`
**Resolution:** Set `"file_delimiter": "|"` in CU config
