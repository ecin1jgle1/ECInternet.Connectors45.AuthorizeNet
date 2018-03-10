using System;
using System.Diagnostics;
using System.Threading;
using Realisable.Log;
using Realisable.Utils;

namespace ECInternet.Connectors45.Authorize_Net
{
	internal static class TraceWriter
	{
		private const string FILE_NAME = "SampleLog.txt";
		private const string TRACE_FLAG = "SAMPLETRACE";
		private const string MESSAGE_FORMAT = "{0} - {1}";

		private static bool _trace;
		private static bool _initialized;

		private static bool Trace
		{
			get
			{
				if (!_initialized)
				{
					if (ConfigurationUtil.HasConfigSetting(TRACE_FLAG))
					{
						_trace = Converter.ConvertTo<bool>(ConfigurationUtil.ConfigurationSetting(TRACE_FLAG));
					}

					_initialized = true;
				}

				return _trace;
			}
		}

		public static void WriteException(Exception e)
		{
			Write(e.Message);
		}

		public static void WriteError(string message, params object[] values)
		{
			WriteError(string.Format(message, values));
		}

		public static void WriteError(string message)
		{
		}

		public static void Write(string message, params object[] values)
		{
			Write(string.Format(message, values));
		}

		public static void Write(string message)
		{
			if (Trace)
			{
				message = Thread.CurrentThread.ManagedThreadId + " - " + message;
				if (Debugger.IsAttached)
				{
					Debug.WriteLine(message);
				}

				// Don't kill IMan if writing to file throws an error.
				try
				{
					TraceUtility.WriteToTrace(FILE_NAME, message);
				}
				catch
				{
				}
			}
		}
	}
}
