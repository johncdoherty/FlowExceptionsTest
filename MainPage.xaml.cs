using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace FlowExceptionsTest;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		BindingContext = new MainViewModel();
	}

	// Async void test - for comparison
	private async void OnAsyncVoidTest(object sender, EventArgs e)
	{
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("🧪 TEST: Async Void Exception (for comparison)");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		
		await Task.Delay(100);
		throw new Exception("TEST: Async void exception - should go to AppDomain handler");
	}
}

public partial class MainViewModel : ObservableObject
{
	[ObservableProperty]
	private string resultText = string.Empty;

	[ObservableProperty]
	private Color resultColor = Colors.Gray;

	[ObservableProperty]
	private bool hasResult = false;

	// Test 1: WITH FlowExceptionsToTaskScheduler = true (Async)
	[RelayCommand(FlowExceptionsToTaskScheduler = true)]
	private async Task ThrowWithFlowExceptions()
	{
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("🧪 TEST 1: Async Task WITH FlowExceptionsToTaskScheduler = true");
		Debug.WriteLine("   Expected: Exception flows to TaskScheduler.UnobservedTaskException");
		Debug.WriteLine($"   Thread ID: {Environment.CurrentManagedThreadId}");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		
		await Task.Delay(100);
		
		ResultText = "Test 1 started. Wait for GC...";
		ResultColor = Colors.Blue;
		HasResult = true;
		
		Debug.WriteLine("🧪 TEST 1: About to throw exception...");
		
		// This exception should flow to TaskScheduler.UnobservedTaskException
		// when the Task is garbage collected
		throw new Exception("TEST 1: Async exception WITH FlowExceptionsToTaskScheduler");
	}

	// Test 2: WITHOUT FlowExceptionsToTaskScheduler (default behavior)
	[RelayCommand]
	private async Task ThrowWithoutFlowExceptions()
	{
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("🧪 TEST 2: Async Task WITHOUT FlowExceptionsToTaskScheduler");
		Debug.WriteLine("   Expected: Exception may crash app or be caught by MAUI handler");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		
		await Task.Delay(100);
		
		ResultText = "Test 2 started. Watch for crash...";
		ResultColor = Colors.Red;
		HasResult = true;
		
		// This exception will NOT flow to TaskScheduler
		// Behavior depends on how the command is invoked
		throw new Exception("TEST 2: Async exception WITHOUT FlowExceptionsToTaskScheduler");
	}

	// Test 3: Sync Task-returning method WITH FlowExceptions
	[RelayCommand(FlowExceptionsToTaskScheduler = true)]
	private Task ThrowSyncWithFlowExceptions()
	{
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("🧪 TEST 3: Synchronous Task WITH FlowExceptionsToTaskScheduler = true");
		Debug.WriteLine("   Expected: Exception flows to TaskScheduler.UnobservedTaskException");
		Debug.WriteLine("   NOTE: Cannot access UI synchronously (would crash with UIKitThreadAccessException)");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		
		// Update UI on main thread (cannot access UI synchronously in non-async method)
		MainThread.BeginInvokeOnMainThread(() =>
		{
			ResultText = "Test 3 started. Wait for GC...";
			ResultColor = Colors.Blue;
			HasResult = true;
		});
		
		// Return a faulted task (synchronous throw)
		throw new Exception("TEST 3: Sync exception WITH FlowExceptionsToTaskScheduler");
	}

	// Test 4: Sync Task-returning method WITHOUT FlowExceptions
	[RelayCommand]
	private Task ThrowSyncWithoutFlowExceptions()
	{
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("🧪 TEST 4: Synchronous Task WITHOUT FlowExceptionsToTaskScheduler");
		Debug.WriteLine("   Expected: Exception may crash app immediately");
		Debug.WriteLine("   NOTE: Cannot access UI synchronously (would crash with UIKitThreadAccessException)");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		
		// Update UI on main thread
		MainThread.BeginInvokeOnMainThread(() =>
		{
			ResultText = "Test 4 started. Watch for crash...";
			ResultColor = Colors.Red;
			HasResult = true;
		});
		
		throw new Exception("TEST 4: Sync exception WITHOUT FlowExceptionsToTaskScheduler");
	}

	// Test 5: Nested async call WITH FlowExceptions
	[RelayCommand(FlowExceptionsToTaskScheduler = true)]
	private async Task ThrowNestedWithFlowExceptions()
	{
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("🧪 TEST 5: Nested async call WITH FlowExceptionsToTaskScheduler = true");
		Debug.WriteLine("   Expected: Exception flows to TaskScheduler.UnobservedTaskException");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		
		ResultText = "Test 5 started. Wait for GC...";
		ResultColor = Colors.Blue;
		HasResult = true;
		
		await NestedMethodThatThrows();
	}

	// Test 6: Task.Run scenario WITH FlowExceptions
	[RelayCommand(FlowExceptionsToTaskScheduler = true)]
	private async Task ThrowInTaskRunWithFlowExceptions()
	{
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("🧪 TEST 6: Task.Run WITH FlowExceptionsToTaskScheduler = true");
		Debug.WriteLine("   Expected: Exception flows to TaskScheduler.UnobservedTaskException");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		
		ResultText = "Test 6 started. Wait for GC...";
		ResultColor = Colors.Blue;
		HasResult = true;
		
		await Task.Run(async () =>
		{
			await Task.Delay(100);
			throw new Exception("TEST 6: Exception in Task.Run WITH FlowExceptionsToTaskScheduler");
		});
	}

	// Test 7: Fire-and-forget scenario WITH FlowExceptions
	[RelayCommand(FlowExceptionsToTaskScheduler = true)]
	private async Task FireAndForgetWithFlowExceptions()
	{
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("🧪 TEST 7: Fire-and-forget WITH FlowExceptionsToTaskScheduler = true");
		Debug.WriteLine("   Expected: Exception flows to TaskScheduler.UnobservedTaskException");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		
		ResultText = "Test 7 started. Wait for GC...";
		ResultColor = Colors.Blue;
		HasResult = true;
		
		// Start a task but don't await it (fire-and-forget)
		_ = Task.Run(async () =>
		{
			await Task.Delay(100);
			throw new Exception("TEST 7: Fire-and-forget exception WITH FlowExceptionsToTaskScheduler");
		});
		
		// Complete immediately
		await Task.CompletedTask;
	}

	// Test 8: Immediate throw (before any await) WITH FlowExceptions
	[RelayCommand(FlowExceptionsToTaskScheduler = true)]
	private async Task ThrowImmediateWithFlowExceptions()
	{
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("🧪 TEST 8: Immediate throw (before await) WITH FlowExceptionsToTaskScheduler = true");
		Debug.WriteLine("   Expected: Exception flows to TaskScheduler.UnobservedTaskException");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		
		ResultText = "Test 8 started. Wait for GC...";
		ResultColor = Colors.Blue;
		HasResult = true;
		
		// Throw BEFORE any await (synchronous exception in async method)
		throw new Exception("TEST 8: Immediate throw WITH FlowExceptionsToTaskScheduler");
		
		await Task.Delay(100); // Never reached
	}

	// Helper method for nested test
	private async Task NestedMethodThatThrows()
	{
		await Task.Delay(100);
		throw new Exception("TEST 5: Nested exception WITH FlowExceptionsToTaskScheduler");
	}

	// Force garbage collection to trigger UnobservedTaskException
	[RelayCommand]
	private void ForceGC()
	{
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("🗑️  Forcing garbage collection...");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		
		Debug.WriteLine("✅ GC Complete. If TaskScheduler handler was triggered, you should see ✅ SUCCESS above.");
		Debug.WriteLine("   If you DON'T see ✅ SUCCESS, then FlowExceptionsToTaskScheduler is NOT working.");
		Debug.WriteLine("   This means exceptions are being silently swallowed or handled elsewhere.");
		
		ResultText = "GC triggered. Check debug output!";
		ResultColor = Colors.Purple;
		HasResult = true;
	}

	// DIAGNOSTIC: Test that TaskScheduler handler actually works
	[RelayCommand]
	private async Task TestTaskSchedulerHandler()
	{
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("🔧 DIAGNOSTIC: Testing TaskScheduler handler directly");
		Debug.WriteLine("   Creating a Task that will be abandoned (unobserved)");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		
		// Create an unobserved task the old-fashioned way (not through RelayCommand)
		_ = Task.Run(async () =>
		{
			await Task.Delay(100);
			throw new Exception("DIAGNOSTIC: Direct unobserved task exception");
		});
		
		ResultText = "Diagnostic test started. Wait then Force GC...";
		ResultColor = Colors.Gray;
		HasResult = true;
		
		await Task.CompletedTask;
	}
}
