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
		// Handler for Task-based exceptions (unobserved tasks)
		TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

		// Handler for AppDomain exceptions (async void, unhandled exceptions)
		AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
	}

	private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
	{
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
		Debug.WriteLine("✅ SUCCESS: TaskScheduler.UnobservedTaskException FIRED!");
		Debug.WriteLine($"   Exception: {e.Exception?.GetBaseException().Message}");
		Debug.WriteLine($"   Type: {e.Exception?.GetBaseException().GetType().Name}");
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
		Debug.WriteLine($"   IsTerminating: {e.IsTerminating}");
		Debug.WriteLine("   This means exception did NOT flow to TaskScheduler");
		Debug.WriteLine("   App may crash...");
		Debug.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
	}
}