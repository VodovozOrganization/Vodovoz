using System;
using QS.ErrorReporting.GtkUI;
using QSSupportLib;
using Vodovoz.Services;

namespace Vodovoz.Infrastructure
{
	public class ErrorReportSettings: IErrorReportingSettings
	{
		private readonly IErrorSendParameterProvider defaultDatabaseProvider;
		private readonly string baseName;

		public ErrorReportSettings(IErrorSendParameterProvider defaultDatabaseProvider, string baseName)
		{
			this.defaultDatabaseProvider = defaultDatabaseProvider;
			this.baseName = baseName;
		}

		public bool SendAutomatically => baseName == defaultDatabaseProvider.GetDefaultBaseForErrorSend();

		public int? LogRowCount => defaultDatabaseProvider.GetRowCountForErrorLog();

		public bool RequestEmail => false;

		public bool RequestDescription => true;
	}
}
