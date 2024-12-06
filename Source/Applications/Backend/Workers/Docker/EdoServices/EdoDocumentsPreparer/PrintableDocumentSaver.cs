using fyiReporting.RDL;
using MySqlConnector;
using QS.Report;
using RdlEngine;

namespace EdoDocumentsPreparer
{
	public class PrintableDocumentSaver
	{
		private readonly string _connectionString;

		public PrintableDocumentSaver(MySqlConnectionStringBuilder connectionStringBuilder)
		{
			_connectionString = (connectionStringBuilder ?? throw new System.ArgumentNullException(nameof(connectionStringBuilder)))
				.ConnectionString;
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
