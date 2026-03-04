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

	// Test 1: WITH FlowExceptionsToTaskScheduler = true
	[RelayCommand(FlowExceptionsToTaskScheduler = true)]
	private async Task ThrowWithFlowExceptions()
	{
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("🧪 TEST 1: Throwing exception WITH FlowExceptionsToTaskScheduler = true");
		Debug.WriteLine("   Expected: Exception flows to TaskScheduler.UnobservedTaskException");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		
		await Task.Delay(100);
		
		ResultText = "Test 1 started. Wait for GC...";
		ResultColor = Colors.Blue;
		HasResult = true;
		
		// This exception should flow to TaskScheduler.UnobservedTaskException
		// when the Task is garbage collected
		throw new Exception("TEST 1: Exception WITH FlowExceptionsToTaskScheduler");
	}

	// Test 2: WITHOUT FlowExceptionsToTaskScheduler (default behavior)
	[RelayCommand]
	private async Task ThrowWithoutFlowExceptions()
	{
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("🧪 TEST 2: Throwing exception WITHOUT FlowExceptionsToTaskScheduler");
		Debug.WriteLine("   Expected: Exception may crash app or be caught by MAUI handler");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		
		await Task.Delay(100);
		
		ResultText = "Test 2 started. Watch for crash...";
		ResultColor = Colors.Red;
		HasResult = true;
		
		// This exception will NOT flow to TaskScheduler
		// Behavior depends on how the command is invoked
		throw new Exception("TEST 2: Exception WITHOUT FlowExceptionsToTaskScheduler");
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
		
		Debug.WriteLine("✅ GC Complete. Check for TaskScheduler.UnobservedTaskException above.");
		
		ResultText = "GC triggered. Check debug output!";
		ResultColor = Colors.Purple;
		HasResult = true;
	}
}
