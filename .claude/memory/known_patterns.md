# Reusable Transform Patterns

---

## Balance / Monetary Fields
```json
{ "source_col": "Bal", "target_col": "account_balance", "transform": "trim, to_decimal" }
```
`DecimalTransformer` automatically strips `$` and `,` before parsing.

---

## Single-Character Status Codes
```json
{ "source_col": "Status", "target_col": "member_status", "transform": "value_map:A->ACTIVE,I->INACTIVE,C->CLOSED" }
```

---

## Full-Word Status (identity map)
```json
{ "source_col": "MBR_STATUS", "target_col": "member_status", "transform": "value_map:ACTIVE->ACTIVE,INACTIVE->INACTIVE,CLOSED->CLOSED" }
```
Or simply:
```json
{ "source_col": "MBR_STATUS", "target_col": "member_status", "transform": "trim" }
```

---

## US Date Format
```json
{ "source_col": "DOB", "target_col": "date_of_birth", "transform": "date:MM/dd/yyyy" }
```

## ISO Date Format
```json
{ "source_col": "BIRTH_DATE", "target_col": "date_of_birth", "transform": "date:yyyy-MM-dd" }
```

---

## Auto-set as_of_date to now
```json
{ "source_col": null, "target_col": "as_of_date", "transform": "default:utcnow" }
```

---

## Phone — strip +1 country code
```json
{ "source_col": "PHONE", "target_col": "phone", "transform": "strip_prefix:+1" }
```

---

## Required Fields Template
```json
{ "source_col": "MemberID", "target_col": "source_member_id", "transform": "trim", "required": true }
```
Always mark `source_member_id` as required.
