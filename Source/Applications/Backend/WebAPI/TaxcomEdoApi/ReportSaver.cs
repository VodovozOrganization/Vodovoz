using System.IO;
using fyiReporting.RDL;
using QS.Report;
using QSProjectsLib;
using RdlEngine;

namespace TaxcomEdoApi
{
	public class PrintableDocumentSaver
	{
		public byte[] SaveToPdf(IPrintableRDLDocument document)
		{
			var ri = document.GetReportInfo();

			using(MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(),
					  QSMain.ConnectionString, OutputPresentationType.PDF, true))
			{
				return stream.GetBuffer();
			}
		}
	}
}
