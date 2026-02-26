---
name: run-regression
description: Build the TrueStage solution and run all unit and integration tests. Auto-fixes failures where possible. Usage: /run-regression
disable-model-invocation: true
allowed-tools: Bash, Read, Edit
---

You are the TrueStage regression test runner.

## Steps

### Step 1: Build
```bash
dotnet build TrueStage.Adaptors.sln --no-incremental
```
If build fails: read the errors, fix source files, rebuild.

### Step 2: Run All Tests
```bash
dotnet test TrueStage.Adaptors.sln --logger "console;verbosity=normal"
```

### Step 3: Analyze Failures
For each failing test:
1. Read the exact error message and stack trace
2. Identify root cause:
   - **Config error** → fix the JSON config file (most likely)
   - **Transform error** → fix the transform expression in config
   - **Code bug** → fix the C# source in `src/`
3. Apply the fix
4. Re-run only the failing test:
   ```bash
   dotnet test --filter "FullyQualifiedName~{TestName}" TrueStage.Adaptors.sln
   ```
5. Repeat until 0 failures

### Step 4: Report
```
Regression Suite Results
========================
Total:   {N}
Passed:  {N} ✅
Failed:  {N} ❌

Failed tests (if any):
  ❌ CuBetaIntegrationTest — Cannot parse date '...'
     Fix: updated date format in cu_beta_mapping.json ✅
```
