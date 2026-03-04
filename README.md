# FlowExceptionsToTaskScheduler Test Project

This test project verifies whether CommunityToolkit.Mvvm's `FlowExceptionsToTaskScheduler` parameter actually works as expected.

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

2. **Test 1: WITH FlowExceptions**
   - Click the GREEN button: "Throw Exception WITH FlowExceptions"
   - Wait 2-3 seconds
   - Click the PURPLE button: "Force Garbage Collection"
   - **Look for this in debug output:**
     ```
     ✅ SUCCESS: TaskScheduler.UnobservedTaskException FIRED!
     ```
   - You should also see an alert dialog saying "FlowExceptionsToTaskScheduler WORKS!"

3. **Test 2: WITHOUT FlowExceptions**
   - Click the RED button: "Throw Exception WITHOUT FlowExceptions"
   - **Expected:** App may crash OR MAUI handler catches it
   - Check debug output for which handler caught it

4. **Test 3: Async Void (for comparison)**
   - Click the ORANGE button: "Throw Exception in Async Void"
   - **Expected:** Should go to `AppDomain.UnhandledException` (different behavior than Test 1)

## Expected Results

### ✅ If FlowExceptionsToTaskScheduler WORKS:
```
Test 1: ✅ SUCCESS: TaskScheduler.UnobservedTaskException FIRED!
Test 2: ❌ FAILURE: App crashes or MAUI handler catches
Test 3: ⚠️  AppDomain.UnhandledException fires
```

### ❌ If FlowExceptionsToTaskScheduler DOES NOT WORK:
```
Test 1: ❌ App crashes or MAUI handler catches (same as Test 2)
Test 2: ❌ App crashes or MAUI handler catches
Test 3: ⚠️  AppDomain.UnhandledException fires
```

## What to Do with Results

### If Test 1 Shows ✅ SUCCESS:
- FlowExceptionsToTaskScheduler parameter IS VALID and WORKS
- Proceed with the recommendations in FAULT_TOLERANCE_ANALYSIS.md
- 40% effort reduction is REAL
- Timeline reduction to 13 weeks is ACCURATE

### If Test 1 Shows ❌ FAILURE:
- FlowExceptionsToTaskScheduler does NOT work (or doesn't exist)
- **MUST UPDATE FAULT_TOLERANCE_ANALYSIS.md** to remove all FlowExceptions references
- Revert to original plan: manual try/catch for ALL 300+ RelayCommands
- Timeline reverts to 18 weeks, 40-58 engineering weeks
- Notify the team immediately that the 40% savings claim was incorrect

## Debug Output Key

- `✅ SUCCESS` = TaskScheduler handler caught the exception (FlowExceptions WORKS!)
- `❌ FAILURE` = AppDomain handler caught it OR app crashed (FlowExceptions FAILED)
- `⚠️  MAUI` = MAUI's handler caught it (platform-specific behavior)

## Notes

- The `ForceGC()` button is critical - `TaskScheduler.UnobservedTaskException` only fires when the Task is garbage collected
- Wait a few seconds between clicking Test 1 and Force GC
- If you don't see ANY exception handlers fire, check that you're looking at the Debug Output window
- Some platforms (Windows) may behave differently than others

## Files Created

```
FlowExceptionsTest/
├── FlowExceptionsTest.csproj
├── MauiProgram.cs
├── App.xaml
├── App.xaml.cs              ← Exception handlers configured here
├── AppShell.xaml
├── AppShell.xaml.cs
├── MainPage.xaml            ← Test UI
├── MainPage.xaml.cs         ← Test commands with/without FlowExceptions
├── Resources/
│   └── Styles/
│       ├── Colors.xaml
│       └── Styles.xaml
└── README.md
```

## Dependencies

- .NET 10.0
- Microsoft.Maui.Controls 10.0.0
- CommunityToolkit.Mvvm 8.4.0

## Quick Test Command

```bash
# Build and run on Mac
cd /Applications/Melbourne/FlowExceptionsTest && \
dotnet build -f net10.0-maccatalyst && \
dotnet build -t:Run -f net10.0-maccatalyst
```
