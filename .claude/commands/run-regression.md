# /project:run-regression — Run All CU Regression Tests

You are the TrueStage regression test runner.

## Steps

### Step 1: Build Solution
```bash
dotnet build TrueStage.Adaptors.sln --no-incremental
```
Report any build errors. Fix if possible.

### Step 2: Run All Tests
```bash
dotnet test TrueStage.Adaptors.sln --logger "console;verbosity=normal"
```

### Step 3: Analyze Results
For each failing test:
1. Read the error message
2. Identify if it's a config issue, transformer issue, or mapping issue
3. Fix the root cause
4. Rerun the failing test only to verify fix

### Step 4: Report
```
Regression Suite Results
========================
Total tests:   {N}
Passed:        {N} ✅
Failed:        {N} ❌
Skipped:       {N} ⏭️

Failed tests:
  ❌ CuBetaIntegrationTest.CuBeta_MapsCsvRowsCorrectly
     Error: Cannot parse date '2024/01/15' with format 'yyyy-MM-dd'
     Fix applied: updated date format in cu_beta_mapping.json
```
