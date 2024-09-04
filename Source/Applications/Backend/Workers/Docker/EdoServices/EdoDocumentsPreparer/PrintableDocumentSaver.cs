using fyiReporting.RDL;
using Microsoft.Extensions.Configuration;
using QS.Report;
using RdlEngine;

namespace EdoDocumentsPreparer
{
	public class PrintableDocumentSaver
	{
		private readonly string _connectionString;

		public PrintableDocumentSaver(IConfiguration configuration)
		{
			_connectionString = (configuration ?? throw new System.ArgumentNullException(nameof(configuration)))
				.GetConnectionString("DefaultConnection");
		}

		public byte[] SaveToPdf(IPrintableRDLDocument document)
		{
			var ri = document.GetReportInfo(_connectionString);

			using var stream = ReportExporter.ExportToMemoryStream(
				ri.GetReportUri(),
				ri.GetParametersString(),
				_connectionString,
				OutputPresentationType.PDF,
				true);
			return stream.GetBuffer();
		}
	}
}
