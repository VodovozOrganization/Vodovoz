using System;
using System.Linq;
using QS.ErrorReporting;
using QS.Project.Domain;
using QS.Project.Versioning;

namespace Vodovoz.Tools
{
	public class ErrorReporter : IErrorReporter
	{
		private static ErrorReporter _instance;

		public static ErrorReporter Instance
		{
			get
			{
				if(_instance == null)
				{
					_instance = new ErrorReporter(new ErrorReportingService(), new ApplicationVersionInfo(), new LogService());
				}
				return _instance;
			}
		}

		private readonly IErrorReportSender _reportSender;
		private readonly ILogService _logService;

		public ErrorReporter(
			IErrorReportSender reportSender,
			IApplicationInfo applicationInfo,
			ILogService logService)
		{
			if(applicationInfo is null)
			{
				throw new ArgumentNullException(nameof(applicationInfo));
			}

			_reportSender = reportSender ?? throw new ArgumentNullException(nameof(reportSender));
			_logService = logService ?? throw new ArgumentNullException(nameof(logService));

			ProductName = applicationInfo.ProductName;
			Version = applicationInfo.Version.ToString();
			Edition = applicationInfo.Modification;
		}

		public string DatabaseName { get; set; }
		public string ProductName { get; protected set; }
		public string Version { get; protected set; }
		public string Edition { get; protected set; }
		public bool AutomaticallySendEnabled { get; set; }
		public int SendedLogRowCount { get; set; }

		public bool AutomaticSendErrorReport(string description, params Exception[] exceptions)
		{
			return SendErrorReport(exceptions, ReportType.Automatic, description, "", null, SendedLogRowCount);
		}

		public bool AutomaticSendErrorReport(string description, UserBase user, params Exception[] exceptions)
		{
			return SendErrorReport(exceptions, ReportType.Automatic, description, "", user, SendedLogRowCount);
		}

		public bool AutomaticSendErrorReport(string description, string email, UserBase user, params Exception[] exceptions)
		{
			return SendErrorReport(exceptions, ReportType.Automatic, description, email, user, SendedLogRowCount);
		}

		public bool ManuallySendErrorReport(string description, string email, UserBase user, params Exception[] exceptions)
		{
			return SendErrorReport(exceptions, ReportType.User, description, email, user, SendedLogRowCount);
		}

		private bool SendErrorReport(
			Exception[] exceptions, 
			ReportType errorReportType, 
			string description, 
			string email, 
			UserBase user,
			int logRowsCount
		)
		{
			if(errorReportType == ReportType.Automatic && !AutomaticallySendEnabled) {
				return false;
			}

			var errorRequest = new SubmitErrorRequest
			{
				ReportType = errorReportType,
				App = new AppInfo
				{
					Modification = Edition,
					Version = Version,
					ProductCode = 3
				},
				Db = new DatabaseInfo
				{
					Name = DatabaseName
				},
				User = new UserInfo
				{
					Name = user == null ? "" : user.Name,
					Email = email ?? string.Empty
				},
				Report = new ErrorInfo
				{
					UserDescription = description ?? string.Empty,
					Log = _logService.GetLog(logRowsCount),
					StackTrace = GetExceptionText(exceptions)
				}
			};
			return _reportSender.SubmitErrorReport(errorRequest);
		}
		
		public static string GetExceptionText(Exception[] exceptions)
		{
			return string.Join("\n Следующее исключение:\n", exceptions.Select(ex => ex.ToString()));
		}
	}
}
