using fyiReporting.RDL;
using QS.Report;
using QSProjectsLib;
using RdlEngine;
using System.IO;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class ReportExporterAdapter : IReportExporter 
	{
		public void ExportReport(IPrintableRDLDocument printableRDLDocument, string path, bool hideSignature)
		{
			var reportInfo = printableRDLDocument.GetReportInfo(QSMain.ConnectionString);

			(printableRDLDocument as ISignableDocument).HideSignature = hideSignature;

			using(MemoryStream stream = ReportExporter.ExportToMemoryStream(reportInfo.GetReportUri(), reportInfo.GetParametersString(), QSMain.ConnectionString, OutputPresentationType.PDF, overwriteSubreportCon:true))
			{
				File.WriteAllBytes(path, stream.GetBuffer());
			}
		}
	}
}
