using fyiReporting.RDL;
using MySqlConnector;
using QS.Report;
using RdlEngine;

namespace Edo.InformalOrderDocuments
{
	/// <summary>
	/// Сохраняет печатаемые документы в PDF
	/// </summary>
	public class PrintableDocumentSaver : IPrintableDocumentSaver
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

			var stream = ReportExporter.ExportToMemoryStream(
				ri.GetReportUri(),
				ri.GetParametersString(),
				_connectionString,
				OutputPresentationType.PDF,
				true);

			try
			{
				return stream.GetBuffer();
			}
			finally
			{
				stream?.Dispose();
			}
		}
	}
}

