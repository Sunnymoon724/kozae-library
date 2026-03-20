using System;

public static class LogBridge
{
	public static Action<string> OnInfo = _ => { };
	public static Action<string> OnWarning = _ => { };
	public static Action<string> OnError = _ => { };

	public static void I(string message)
	{
		OnInfo.Invoke(message);
	}

	public static void W(string message)
	{
		OnWarning.Invoke(message);
	}

	public static void E(string message)
	{
		OnError.Invoke(message);
	}
}