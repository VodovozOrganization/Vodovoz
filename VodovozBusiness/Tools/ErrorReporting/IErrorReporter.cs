using System;
using QS.ErrorReporting;
using QS.Project.Domain;

namespace Vodovoz.Tools
{
	public interface IErrorReporter
	{
		string DatabaseName { get; }
		string ProductName { get; }
		string Version { get; }
		string Edition { get; }
		bool CanSendAutomatically { get; }

		bool SendErrorReport(
			Exception[] exceptions,
			ErrorReportType errorReportType = ErrorReportType.Automatic,
			string description = null,
			string email = null,
			UserBase user = null
		);

		bool SendErrorReport(
			Exception[] exceptions,
			int logRowCount,
			ErrorReportType errorReportType = ErrorReportType.Automatic,
			string description = null,
			string email = null,
			UserBase user = null
		);
	}
}
