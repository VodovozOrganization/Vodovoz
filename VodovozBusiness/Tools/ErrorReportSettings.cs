using System;
using QS.DomainModel.UoW;
using QS.ErrorReporting;
using QS.Project.Domain;
using QS.Services;
using Vodovoz.Services;

namespace Vodovoz.Tools
{
	public class ErrorReportSettings: IErrorReportingSettings
	{
		/// <summary>
		/// Автоматически заполняет пользователя, параметр автоматической отправки, базу, и кол-во строк лога
		/// </summary>
		public ErrorReportSettings(IErrorSendParameterProvider defaultDatabaseProvider,
			Exception exception,
			IUnitOfWork uow,
			IUserService userService,
			string currentDatabase,
			ErrorReportType reportType = ErrorReportType.Automatic
			)
		{
			LogRowCount = defaultDatabaseProvider.GetRowCountForErrorLog();
			User = userService.GetCurrentUser(uow);
			Email = User?.Email;
			Exception = exception;
			ReportType = reportType;
			CanSendAutomatically = defaultDatabaseProvider.GetDefaultBaseForErrorSend() == currentDatabase;
		}

		public int? LogRowCount { get; set; }
		public string Description { get; set; }
		public string Email { get; set; }
		public ErrorReportType ReportType { get; set; }
		public UserBase User { get; set; }
		public Exception Exception { get; set; }
		public bool CanSendAutomatically { get; set; }
	}
}
