using System.Diagnostics;

namespace FlowExceptionsTest;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// Configure global exception handlers
		ConfigureExceptionHandlers();

		MainPage = new AppShell();
	}

	private void ConfigureExceptionHandlers()
	{
		// Handler for EVERY exception (fires before any catch handlers)
		AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;

		// Handler for Task-based exceptions (unobserved tasks)
		TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

		// Handler for AppDomain exceptions (async void, unhandled exceptions)
		AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

		// DIAGNOSTIC: Verify handlers are registered
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("🔧 DIAGNOSTIC: Exception handlers registered");
		Debug.WriteLine($"   FirstChance handler: {nameof(OnFirstChanceException)}");
		Debug.WriteLine($"   TaskScheduler handler: {nameof(OnUnobservedTaskException)}");
		Debug.WriteLine($"   AppDomain handler: {nameof(OnUnhandledException)}");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
	}

	private void OnFirstChanceException(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
	{
		// Filter out noise - only log our test exceptions
		if (e.Exception.Message.Contains("TEST"))
		{
			Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
			Debug.WriteLine("🔍 FIRST CHANCE: Exception thrown (before any handlers)");
			Debug.WriteLine($"   Exception: {e.Exception.Message}");
			Debug.WriteLine($"   Type: {e.Exception.GetType().Name}");
			Debug.WriteLine($"   Thread ID: {Environment.CurrentManagedThreadId}");
			Debug.WriteLine($"   StackTrace Preview: {e.Exception.StackTrace?.Split('\n').FirstOrDefault()?.Trim()}");
			Debug.WriteLine("   Note: This fires before any catch handlers run");
			Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		}
	}

	private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
	{
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("✅ SUCCESS: TaskScheduler.UnobservedTaskException FIRED!");
		Debug.WriteLine($"   Exception: {e.Exception?.GetBaseException().Message}");
		Debug.WriteLine($"   Type: {e.Exception?.GetBaseException().GetType().Name}");
		Debug.WriteLine($"   Sender: {sender?.GetType().Name ?? "null"}");
		Debug.WriteLine($"   Thread ID: {Environment.CurrentManagedThreadId}");
		Debug.WriteLine("   This means FlowExceptionsToTaskScheduler = true WORKS!");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		
		// Mark as observed to prevent app crash
		e.SetObserved();
		
		MainThread.BeginInvokeOnMainThread(() =>
		{
			MainPage?.DisplayAlert("✅ TaskScheduler Handler", 
				$"FlowExceptionsToTaskScheduler WORKS!\n\nException caught: {e.Exception?.GetBaseException().Message}", 
				"OK");
		});
	}

	private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("❌ FAILURE: AppDomain.UnhandledException FIRED!");
		Debug.WriteLine($"   Exception: {(e.ExceptionObject as Exception)?.Message}");
		Debug.WriteLine($"   Exception Type: {e.ExceptionObject?.GetType().Name}");
		Debug.WriteLine($"   IsTerminating: {e.IsTerminating}");
		Debug.WriteLine($"   Thread ID: {Environment.CurrentManagedThreadId}");
		Debug.WriteLine("   This means exception did NOT flow to TaskScheduler");
		Debug.WriteLine("   App may crash...");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		
		MainThread.BeginInvokeOnMainThread(() =>
		{
			MainPage?.DisplayAlert("❌ AppDomain Handler", 
				$"Exception went to AppDomain handler (not TaskScheduler)\n\nException: {(e.ExceptionObject as Exception)?.Message}", 
				"OK");
		});
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = base.CreateWindow(activationState);
		
		// DIAGNOSTIC: Log window creation
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("🔧 DIAGNOSTIC: Window created, exception handlers active");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		
		return window;
	}
}