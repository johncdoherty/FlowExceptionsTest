# FlowExceptionsToTaskScheduler Test Project

**A comprehensive test harness to validate CommunityToolkit.Mvvm's `FlowExceptionsToTaskScheduler` parameter before implementing workspace-wide changes.**

---

## 🚀 Quick Start (TL;DR)

```bash
# 1. Run the app
cd /Applications/Melbourne/FlowExceptionsTest
dotnet build -t:Run -f net10.0-maccatalyst

# 2. In the app:
#    - Click any GREEN button (Test 1-8)
#    - Wait 3 seconds
#    - Click PURPLE "Force GC" button
#    
# 3. Watch Debug Console for:
#    ✅ "SUCCESS: TaskScheduler.UnobservedTaskException FIRED!" = WORKS!
#    ❌ "FAILURE: AppDomain.UnhandledException FIRED!" = DOESN'T WORK
```

**Success Criteria:** All 6 GREEN tests should show ✅ SUCCESS → Proceed with 40% effort reduction  
**Failure Criteria:** Any GREEN test shows ❌ FAILURE → Revert to manual try/catch plan

---

## Executive Summary

This test project answers the **critical $250K question**: *Does `[RelayCommand(FlowExceptionsToTaskScheduler = true)]` actually route exceptions to `TaskScheduler.UnobservedTaskException` instead of crashing the app?*

**Impact:**
- ✅ **If YES**: 40% effort reduction (13 weeks vs 18 weeks), 300+ RelayCommands protected with parameter
- ❌ **If NO**: Revert to manual try/catch for all 600+ async methods (18 weeks, 40-58 eng-weeks)

**Test Coverage:** 9 scenarios across all async patterns (async/await, sync Task, nested calls, Task.Run, fire-and-forget, immediate throw, async void)

**Time to Run:** 5 minutes per platform

---

## Purpose

Tests the claim made in FAULT_TOLERANCE_ANALYSIS.md that adding `FlowExceptionsToTaskScheduler = true` to `[RelayCommand]` attributes will route exceptions to `TaskScheduler.UnobservedTaskException` instead of crashing the app.

## How to Run

### macOS (MacCatalyst)
```bash
cd /Applications/Melbourne/FlowExceptionsTest
dotnet build -t:Run -f net10.0-maccatalyst
```

### iOS Simulator
```bash
cd /Applications/Melbourne/FlowExceptionsTest
dotnet build -t:Run -f net10.0-ios
```

### Android Emulator
```bash
cd /Applications/Melbourne/FlowExceptionsTest
dotnet build -t:Run -f net10.0-android
```

### Windows
```bash
cd /Applications/Melbourne/FlowExceptionsTest
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```

## Test Procedure

1. **Launch the app** and watch the Debug Output window (VS Code: Debug Console)

2. **Test GREEN buttons (WITH FlowExceptions)** - All should route to TaskScheduler:
   - **Test 1**: Async Task + FlowExceptions (standard async/await pattern)
   - **Test 3**: Sync Task + FlowExceptions (synchronous throw in Task-returning method)
   - **Test 5**: Nested Async + FlowExceptions (nested async method calls)
   - **Test 6**: Task.Run + FlowExceptions (background work)
   - **Test 7**: Fire-and-Forget + FlowExceptions (unobserved task scenario)
   - **Test 8**: Immediate Throw + FlowExceptions (throw before first await)
   
   After clicking any GREEN button:
   - Wait 2-3 seconds
   - Click the PURPLE button: "Force Garbage Collection"
   - **Look for this in debug output:**
     ```
     ✅ SUCCESS: TaskScheduler.UnobservedTaskException FIRED!
     ```
   - You should also see an alert dialog saying "FlowExceptionsToTaskScheduler WORKS!"

3. **Test RED buttons (WITHOUT FlowExceptions)** - Should crash or go to AppDomain handler:
   - **Test 2**: Async Task WITHOUT (same as Test 1 but no parameter)
   - **Test 4**: Sync Task WITHOUT (same as Test 3 but no parameter)
   - **Expected:** App may crash OR AppDomain handler catches it

4. **Test ORANGE button (Async Void)** - For comparison:
   - **Test 9**: Async Void Exception
   - **Expected:** Should go to `AppDomain.UnhandledException` (different behavior than RelayCommand)

## Expected Results

### ✅ If FlowExceptionsToTaskScheduler WORKS:
All GREEN button tests (1, 3, 5, 6, 7, 8) should show:
```
✅ SUCCESS: TaskScheduler.UnobservedTaskException FIRED!
```

RED button tests (2, 4) should show:
```
❌ FAILURE: App crashes or AppDomain.UnhandledException fires
```

ORANGE button test (9) should show:
```
⚠️  AppDomain.UnhandledException fires (expected for async void)
```

### ❌ If FlowExceptionsToTaskScheduler DOES NOT WORK:
GREEN and RED buttons would behave the same (both crash or go to AppDomain)

## Test Coverage

The test app validates FlowExceptionsToTaskScheduler across these scenarios:

| Test | Scenario | FlowExceptions | Expected Handler |
|------|----------|----------------|------------------|
| 1 | Async Task (await then throw) | ✅ Yes | TaskScheduler |
| 2 | Async Task (await then throw) | ❌ No | AppDomain/Crash |
| 3 | Sync Task (immediate throw) | ✅ Yes | TaskScheduler |
| 4 | Sync Task (immediate throw) | ❌ No | AppDomain/Crash |
| 5 | Nested async calls | ✅ Yes | TaskScheduler |
| 6 | Task.Run background | ✅ Yes | TaskScheduler |
| 7 | Fire-and-forget (unobserved) | ✅ Yes | TaskScheduler |
| 8 | Throw before await | ✅ Yes | TaskScheduler |
| 9 | Async void handler | N/A | AppDomain |

This covers:
- ✅ Standard async/await patterns
- ✅ Synchronous exceptions in Task-returning methods
- ✅ Nested async method chains
- ✅ Background work (Task.Run)
- ✅ Unobserved tasks (fire-and-forget)
- ✅ Immediate exceptions (before first await)
- ✅ Comparison with async void (cannot use FlowExceptions)

## What Makes This Test Definitive

### 1. **Exact Production Environment**
- Same .NET version (10.0) as Melbourne workspace apps
- Same CommunityToolkit.Mvvm version (8.4.0) as Survey123/QuickCapture
- Same MAUI version (10.0.0) as production apps
- Tests on actual target platforms (iOS, MacCatalyst, Android)

### 2. **Complete Pattern Coverage**
Every async pattern used in the Melbourne codebase:
- Standard async/await (most common)
- Synchronous exceptions in Task-returning methods (edge case)
- Nested async method chains (common in business logic)
- Task.Run background work (file operations, processing)
- Fire-and-forget scenarios (telemetry, logging)
- Immediate exceptions before await (validation failures)

### 3. **Controlled A/B Testing**
- Tests 1,3,5,6,7,8: WITH FlowExceptionsToTaskScheduler
- Tests 2,4: WITHOUT (same patterns for direct comparison)
- Test 9: Async void (baseline showing different behavior)

### 4. **Observable Results**
- Debug logging with clear ✅/❌ indicators
- Visual alerts confirming exception routing
- Explicit handler identification (TaskScheduler vs AppDomain)
- No ambiguity in test outcomes

### 5. **Addresses Low-Confidence Concern**
From the conversation summary: "LOW-MEDIUM confidence" based on single unverified usage. This test converts assumption into evidence through systematic validation.

## What to Do with Results

### If ALL GREEN tests show ✅ SUCCESS:
- FlowExceptionsToTaskScheduler parameter IS VALID and WORKS across all scenarios
- Proceed with confidence with the recommendations in FAULT_TOLERANCE_ANALYSIS.md
- 40% effort reduction is REAL (300+ RelayCommands protected with parameter)
- Timeline reduction to 13 weeks is ACCURATE
- Safe to implement global rollout: `[RelayCommand(FlowExceptionsToTaskScheduler = true)]`

### If ANY GREEN test shows ❌ FAILURE:
- FlowExceptionsToTaskScheduler does NOT work as expected (or doesn't work in that scenario)
- **MUST UPDATE FAULT_TOLERANCE_ANALYSIS.md** to remove or qualify FlowExceptions recommendations
- May need manual try/catch for scenarios where FlowExceptions fails
- Document which scenarios work vs don't work
- Timeline may need adjustment based on what scenarios are affected

### If ALL GREEN tests show ❌ FAILURE:
- FlowExceptionsToTaskScheduler does NOT work AT ALL
- Revert to original plan: manual try/catch for ALL 300+ RelayCommands
- Timeline reverts to 18 weeks, 40-58 engineering weeks
- Update FAULT_TOLERANCE_ANALYSIS.md to remove all FlowExceptions references
- Notify the team immediately that the 40% savings claim was incorrect

## Debug Output Key

- `✅ SUCCESS` = TaskScheduler handler caught the exception (FlowExceptions WORKS!)
- `❌ FAILURE` = AppDomain handler caught it OR app crashed (FlowExceptions FAILED)
- `⚠️  MAUI` = MAUI's handler caught it (platform-specific behavior)

## Notes

- The `ForceGC()` button is critical - `TaskScheduler.UnobservedTaskException` only fires when the Task is garbage collected
- Wait a few seconds between clicking any GREEN test and Force GC
- If you don't see ANY exception handlers fire, check that you're looking at the Debug Output window (View → Debug Console in VS Code)
- Some platforms (Windows) may behave differently than others
- Build warnings about deprecated APIs (MainPage, DisplayAlert) are safe to ignore for testing purposes

## Platform Requirements

- **iOS**: Minimum version 14.2
- **MacCatalyst**: Minimum version 15.0 (required by .NET 10.0 SDK)
- **Android**: Minimum API level 21
- **Windows**: Minimum version 10.0.17763.0

## Build Status

✅ Project builds successfully with minor deprecation warnings:
- `Application.MainPage` is deprecated (use `Windows[0].Page` instead)
- `DisplayAlert` is deprecated (use `DisplayAlertAsync` instead)
- Unreachable code after throw statement (expected)

These warnings don't affect the test functionality.

## Project Structure

```
FlowExceptionsTest/
├── FlowExceptionsTest.csproj    ← Project file (minimal config, no custom resources)
├── FlowExceptionsTest.sln       ← Solution file for VS Code
├── MauiProgram.cs               ← App initialization (no custom fonts)
├── App.xaml                     ← App resources
├── App.xaml.cs                  ← ⭐ Exception handlers configured here
│                                   • TaskScheduler.UnobservedTaskException
│                                   • AppDomain.UnhandledException
├── AppShell.xaml                ← Shell navigation
├── AppShell.xaml.cs
├── MainPage.xaml                ← ⭐ Test UI (9 test buttons)
├── MainPage.xaml.cs             ← ⭐ 9 test scenarios
│                                   • 6 WITH FlowExceptionsToTaskScheduler
│                                   • 2 WITHOUT (for comparison)
│                                   • 1 async void (baseline)
├── Platforms/
│   ├── Android/                 ← Android-specific files
│   ├── iOS/                     ← iOS-specific files
│   └── MacCatalyst/             ← MacCatalyst-specific files
├── Resources/
│   └── Styles/
│       ├── Colors.xaml          ← Color definitions
│       └── Styles.xaml          ← UI styles (no custom fonts)
└── README.md                    ← This file
```

**Key Features:**
- Minimal dependencies (no custom fonts, icons, or assets)
- Comprehensive exception handler setup in App.xaml.cs
- 9 distinct test scenarios covering all async patterns
- Visual feedback (color-coded buttons) and debug logging

## Dependencies

- **.NET 10.0** - Latest .NET version
- **Microsoft.Maui.Controls 10.0.0** - Cross-platform UI framework
- **CommunityToolkit.Mvvm 8.4.0** - MVVM helpers (provides `[RelayCommand]` attribute)

**Why CommunityToolkit.Mvvm 8.4.0?**
This is the same version used across the Melbourne workspace (Survey123, QuickCapture, etc.), ensuring the test results accurately reflect production behavior.

## Quick Start Commands

### Build Only
```bash
cd /Applications/Melbourne/FlowExceptionsTest
dotnet build -f net10.0-maccatalyst
```

### Build and Run
```bash
cd /Applications/Melbourne/FlowExceptionsTest
dotnet build -t:Run -f net10.0-maccatalyst
```

### Open in VS Code
```bash
code /Applications/Melbourne/FlowExceptionsTest/FlowExceptionsTest.sln
```

### Clean Build
```bash
cd /Applications/Melbourne/FlowExceptionsTest
dotnet clean && dotnet build -f net10.0-maccatalyst
```

## Troubleshooting

### "SupportedOSPlatformVersion is lower than minimum"
- Fixed: MacCatalyst now requires 15.0 minimum (updated in .csproj)

### "Failed to compute hash for Resources/Splash/splash.svg"
- Fixed: All custom resources removed from project

### "UnhandledExceptionEventArgs does not exist"
- Fixed: Removed MAUI-specific handler (not available in .NET 10.0)

### Debug Output Not Showing
- In VS Code: View → Debug Console
- Make sure app is running in Debug mode
- Check terminal output for exception messages

## Next Steps After Testing

### Scenario A: All Tests Pass ✅
1. Document test results with screenshots/logs
2. Update FAULT_TOLERANCE_ANALYSIS.md status from "unverified" to "validated"
3. Proceed with Quick Start (15-minute global rollout)
4. Begin Phase 1 remediation with confidence
5. Monitor crash telemetry to confirm real-world effectiveness

### Scenario B: Some Tests Fail ⚠️
1. Document which scenarios work vs fail
2. Update FAULT_TOLERANCE_ANALYSIS.md with qualified recommendations
3. Create hybrid approach:
   - Use FlowExceptions for working scenarios
   - Manual try/catch for failing scenarios
4. Adjust timeline based on hybrid workload
5. File issue with CommunityToolkit.Mvvm for failing scenarios

### Scenario C: All Tests Fail ❌
1. Document comprehensive test failure
2. Revert FAULT_TOLERANCE_ANALYSIS.md to original plan
3. Remove all FlowExceptions references from documentation
4. Send honest assessment to stakeholders:
   - 18 weeks, 40-58 engineering weeks
   - Manual try/catch for all 600+ methods
   - No shortcuts available
5. Consider filing issue/PR with CommunityToolkit.Mvvm if parameter truly doesn't exist

### Test Results Template

```markdown
## FlowExceptionsToTaskScheduler Test Results

**Date:** [Date]
**Platform:** [iOS / MacCatalyst / Android / Windows]
**Device:** [Device/Simulator info]
**Tester:** [Name]

### Results

| Test | Scenario | Expected | Actual | Pass/Fail |
|------|----------|----------|--------|-----------|
| 1 | Async Task + Flow | TaskScheduler | [Result] | ✅/❌ |
| 2 | Async Task no Flow | AppDomain | [Result] | ✅/❌ |
| 3 | Sync Task + Flow | TaskScheduler | [Result] | ✅/❌ |
| 4 | Sync Task no Flow | AppDomain | [Result] | ✅/❌ |
| 5 | Nested + Flow | TaskScheduler | [Result] | ✅/❌ |
| 6 | Task.Run + Flow | TaskScheduler | [Result] | ✅/❌ |
| 7 | Fire-forget + Flow | TaskScheduler | [Result] | ✅/❌ |
| 8 | Immediate + Flow | TaskScheduler | [Result] | ✅/❌ |
| 9 | Async void | AppDomain | [Result] | ✅/❌ |

### Conclusion

[Overall assessment: Proceed / Qualify / Revert]

### Debug Output

\`\`\`
[Paste relevant debug output here]
\`\`\`

### Screenshots

[Attach screenshots of alerts/debug console]
```

---

**Created:** March 2026  
**Purpose:** Validate FlowExceptionsToTaskScheduler before $250K remediation project  
**Status:** Ready for testing
