using fyiReporting.RDL;
using QS.Report;
using QSProjectsLib;
using RdlEngine;
using System.IO;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class ReportExporterAdapter : IReportExporter 
	{
		private const string _hideSignatureParameterName = "hide_signature";

		public void ExportReport(IPrintableRDLDocument printableRDLDocument, string path, bool hideSignature)
		{
			var reportInfo = printableRDLDocument.GetReportInfo(QSMain.ConnectionString);

			if(reportInfo.Parameters.ContainsKey(_hideSignatureParameterName))
			{
				reportInfo.Parameters[_hideSignatureParameterName] = hideSignature;
			}

			using(MemoryStream stream = ReportExporter.ExportToMemoryStream(reportInfo.GetReportUri(), reportInfo.GetParametersString(), QSMain.ConnectionString, OutputPresentationType.PDF, overwriteSubreportCon:true))
			{
				File.WriteAllBytes(path, stream.GetBuffer());
			}
		}
	}
}
