using fyiReporting.RDL;
using MySqlConnector;
using QS.Report;
using RdlEngine;

namespace Edo.Docflow
{
	/// <summary>
	/// Интерфейс для сохранения печатаемых документов в PDF
	/// </summary>
	public interface IPrintableDocumentSaver
	{
		/// <summary>
		/// Сохраняет документ в PDF
		/// </summary>
		/// <param name="document">Документ для сохранения</param>
		/// <returns>Массив байтов PDF файла</returns>
		byte[] SaveToPdf(IPrintableRDLDocument document);
	}
	// Куда лучше всего это переместить? PrintableDocumentSaver из EdoDocumentsPreparer не могу использовать, т.к. там .NET Standard 2.0, а тут .NET 5.0
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

