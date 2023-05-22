using fyiReporting.RDL;
using Microsoft.Extensions.Configuration;
using QS.Report;
using RdlEngine;
using System.IO;

namespace TaxcomEdoApi
{
	public class PrintableDocumentSaver
	{
		private readonly string _connectioinString;

		public PrintableDocumentSaver(IConfiguration configuration)
		{
			_connectioinString = (configuration ?? throw new System.ArgumentNullException(nameof(configuration)))
				.GetConnectionString("DefaultConnection");
		}

		public byte[] SaveToPdf(IPrintableRDLDocument document)
		{
			var ri = document.GetReportInfo(_connectioinString);

			using(MemoryStream stream = ReportExporter.ExportToMemoryStream(ri.GetReportUri(), ri.GetParametersString(),
				      _connectioinString, OutputPresentationType.PDF, true))
			{
				return stream.GetBuffer();
			}
		}
	}
}
