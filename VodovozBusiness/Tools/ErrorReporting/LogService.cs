using System;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NLog.Targets;

namespace Vodovoz.Tools
{
	public class LogService : ILogService
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public string GetLog(int? rowCount = null)
		{
			string logFileName = GetLogFilePath();
			string log = String.Empty;
			if(!String.IsNullOrWhiteSpace(logFileName)) {
				try {
					if(rowCount.HasValue) {
						using(StreamReader reader = new StreamReader(logFileName)) {
							var sb = new StringBuilder();
							for(int i = 0; !reader.EndOfStream && i < rowCount.Value; i++) {
								sb.AppendLine(reader.ReadLine());
							}
							log = sb.ToString();
						}
					} else {
						log = File.ReadAllText(logFileName);
					}
				} catch(Exception ex) {
					logger.Error(ex, "Не смогли прочитать лог файл {0}, для отправки.");
				}
			}
			return log;
		}

		private string GetLogFilePath()
		{
			var fileTarget = LogManager.Configuration.AllTargets.FirstOrDefault(t => t is FileTarget) as FileTarget;
			return fileTarget == null ? string.Empty : fileTarget.FileName.Render(new LogEventInfo { Level = LogLevel.Debug });
		}
	}
}
