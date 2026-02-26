# Skill: Run Tests and Auto-Fix Failures

## Steps

### 1. Build
```bash
dotnet build TrueStage.Adaptors.sln
```
If build fails: read errors, fix source files, rebuild.

### 2. Run Unit Tests
```bash
dotnet test tests/TrueStage.Core.UnitTests/TrueStage.Core.UnitTests.csproj --logger "console;verbosity=normal"
```

### 3. Run Integration Tests
```bash
dotnet test tests/TrueStage.Integration.Tests/TrueStage.Integration.Tests.csproj --logger "console;verbosity=normal"
```

### 4. For Each Failure
- Read the exact error message and stack trace
- Identify root cause:
  - **Config error** → fix the JSON config file
  - **Transform error** → check transform expression in config
  - **Code error** → fix the C# source (rare — engine is shared and stable)
- Apply fix
- Re-run only the failing test to confirm fix:
  ```bash
  dotnet test --filter "FullyQualifiedName~{FailingTestName}"
  ```

### 5. Loop Until All Pass
Repeat until `dotnet test TrueStage.Adaptors.sln` shows 0 failures.

### 6. Report
```
Test Run Complete
✅ 15/15 tests passed
   - TrueStage.Core.UnitTests:         12 passed
   - TrueStage.Integration.Tests:       3 passed
```
