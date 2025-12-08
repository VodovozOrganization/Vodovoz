using QS.Report;

namespace Edo.InformalOrderDocuments
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
}

