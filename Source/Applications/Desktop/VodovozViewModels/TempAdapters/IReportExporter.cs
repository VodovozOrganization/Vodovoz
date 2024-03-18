using QS.Report;

namespace Vodovoz.ViewModels.TempAdapters
{
	public interface IReportExporter
	{
		void ExportReport(IPrintableRDLDocument printableRDLDocument, string path, bool hideSignature);
	}
}
