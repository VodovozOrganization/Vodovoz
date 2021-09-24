using System;
using System.Linq;
using QS.ErrorReporting;
using QS.Project.Domain;
using QS.Project.Versioning;

namespace Vodovoz.Tools
{
	public class SingletonErrorReporter : IErrorReporter
	{
		protected SingletonErrorReporter() { }

		private static SingletonErrorReporter instance;
		/// <summary>
		/// При использовании неинициализированного <see cref="SingletonErrorReporter"/> кидает ошибку
		/// </summary>
		public static SingletonErrorReporter Instance {
			get {
				if(instance == null) {
					throw new ArgumentNullException(nameof(instance));
				}
				return instance;
			}
		}

		public static bool IsInitialized => instance != null;

		/// <summary>
		/// Производит инициализацию <see cref="SingletonErrorReporter"/>
		/// </summary>
		/// <param name="sendService">Сервис отправки сообщений</param>
		/// <param name="applicationInfo">Информация о приложении</param>
		/// <param name="logService">Сервис получения лога</param>
		/// <param name="databaseName">Имя текущей базы данных</param>
		/// <param name="canSendAutomatically">Если<c>false</c> и передаваемый в SendErrorReport() <see cref="ErrorReportType"/>, = <see cref="ErrorReportType.Automatic"/> то не отправляет сообщение об ошибке</param>
		/// <param name="autoSendLogRowCount">Если передаваемый в SendErrorReport() <see cref="ErrorReportType"/>, = <see cref="ErrorReportType.Automatic"/>, то отправляет на сервер такое количество лога по умолчанию</param>
		public static void Initialize(
			IErrorReportingService sendService,
			IApplicationInfo applicationInfo,
			ILogService logService = null,
			string databaseName = null,
			bool canSendAutomatically = false,
			int? autoSendLogRowCount = null
		)
		{
			instance = new SingletonErrorReporter {
				sendService = sendService ?? throw new ArgumentNullException(nameof(sendService))
			};
			if(applicationInfo == null) {
				throw new ArgumentNullException(nameof(applicationInfo));
			}
			instance.Edition = applicationInfo.Modification;
			instance.Version = applicationInfo.Version.ToString();
			instance.ProductName = applicationInfo.ProductName;
			instance.DatabaseName = databaseName;
			instance.logService = logService;
			instance.CanSendAutomatically = canSendAutomatically;
			instance.autoSendLogRowCount = autoSendLogRowCount;
		}

		private ILogService logService;
		private int? autoSendLogRowCount;
		private IErrorReportingService sendService;

		public string DatabaseName { get; protected set; }
		public string ProductName { get; protected set; }
		public string Version { get; protected set; }
		public string Edition { get; protected set; }
		public bool CanSendAutomatically { get; protected set; }

		public bool SendErrorReport(
			Exception[] exceptions, 
			ErrorReportType errorReportType, 
			string description, 
			string email, 
			UserBase user
		)
		{
			if(errorReportType == ErrorReportType.Automatic && !CanSendAutomatically) {
				return false;
			}

			var errorReport = new ErrorReport {
				DBName = DatabaseName,
				Edition = Edition,
				Product = ProductName,
				Version = Version,
				Email = email,
				Description = description,
				ReportType = errorReportType,
				StackTrace = GetExceptionText(exceptions),
				UserName = user?.Name
			};

			errorReport = PrepareLog(errorReport);
			return sendService.SubmitErrorReport(errorReport);
		}

		public bool SendErrorReport(
			Exception[] exceptions,
			int logRowsCount,
			ErrorReportType errorReportType,
			string description,
			string email,
			UserBase user
		)
		{
			if(errorReportType == ErrorReportType.Automatic && !CanSendAutomatically) {
				return false;
			}

			var errorReport = new ErrorReport {
				DBName = DatabaseName,
				Edition = Edition,
				Product = ProductName,
				Version = Version,
				Email = email,
				Description = description,
				ReportType = errorReportType,
				StackTrace = GetExceptionText(exceptions),
				UserName = user?.Name
			};

			errorReport = PrepareLog(errorReport, logRowsCount);
			return sendService.SubmitErrorReport(errorReport);
		}

		private ErrorReport PrepareLog(ErrorReport errorReport, int? logRowOverrideCount = null)
		{
			if(logService != null) {
				if(logRowOverrideCount != null) {
					errorReport.LogFile = logService.GetLog(logRowOverrideCount);
				}
				else {
					errorReport.LogFile = errorReport.ReportType == ErrorReportType.Automatic
						? logService.GetLog(autoSendLogRowCount)
						: logService.GetLog();
				}
			}
			return errorReport;
		}

		public static string GetExceptionText(Exception[] exceptions) =>
			string.Join("\n Следующее исключение:\n", exceptions.Select(ex => ex.ToString()));
	}
}
