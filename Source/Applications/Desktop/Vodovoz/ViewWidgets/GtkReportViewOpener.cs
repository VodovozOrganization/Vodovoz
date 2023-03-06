using QS.Report;
using QS.Tdi;
using QSReport;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewWidgets
{
	public class GtkReportViewOpener : IReportViewOpener
	{
		public void OpenReport(ITdiTab tab, ReportInfo reportInfo)
		{
			tab.TabParent.AddTab(new ReportViewDlg(reportInfo),tab);
		}

		public void OpenReport(ITdiTabParent tabParent, ReportInfo reportInfo)
		{
			tabParent.AddTab(new ReportViewDlg(reportInfo), null);
		}

		public void OpenReportInSlaveTab(ITdiTab tab, ReportInfo reportInfo)
		{
			tab.TabParent.AddSlaveTab(tab, new ReportViewDlg(reportInfo));
		}
	}
}
