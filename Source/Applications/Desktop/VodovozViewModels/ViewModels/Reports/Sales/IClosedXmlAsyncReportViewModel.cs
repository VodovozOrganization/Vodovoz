using System.Collections.Generic;
using System.Threading;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public interface IClosedXmlAsyncReportViewModel<TReport>
		where TReport : class
	{
		bool CanCancelGenerate { get; set; }
		bool CanGenerate { get; }
		bool CanSave { get; set; }
		bool IsGenerating { get; set; }
		bool IsSaving { get; set; }
		IEnumerable<string> LastGenerationErrors { get; set; }
		TReport Report { get; set; }
		CancellationTokenSource ReportGenerationCancelationTokenSource { get; set; }
		void ExportReport(string path);
	}
}
