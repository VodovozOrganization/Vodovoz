using System;
using System.IO;
using System.Linq;
using NLog;
using NLog.Targets;

namespace Vodovoz.Tools
{
	public class LogService : ILogService
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		public string GetLog(int? rowCount = null)
		{
			string logFileName = GetLogFilePath();
			if(String.IsNullOrWhiteSpace(logFileName)) {
				return "";
			}
			try {
				if(!rowCount.HasValue) {
					return File.ReadAllText(logFileName);
				}
				
				var allLines = File.ReadAllLines(logFileName);
				return String.Join("\n", allLines.Skip(Math.Max(0, allLines.Length - rowCount.Value)));
			} catch(Exception ex) {
				logger.Error(ex, "Не получилось прочитать лог");
				return "";
			}
		}

		private static string GetLogFilePath()
		{
			var fileTarget = LogManager.Configuration.AllTargets.FirstOrDefault(t => t is FileTarget) as FileTarget;
			return fileTarget == null ? string.Empty : fileTarget.FileName.Render(new LogEventInfo { Level = LogLevel.Debug });
		}
	}
}
