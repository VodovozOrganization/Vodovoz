using System;
using QS.Report;
using QS.Tdi;

namespace Vodovoz.TempAdapters
{
	public interface IReportViewOpener
	{
		void OpenReport(ITdiTab tab, ReportInfo reportInfo);
		void OpenReportInSlaveTab(ITdiTab tab, ReportInfo reportInfo);
	}
}
