namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Информация для отправки документа по ЭДО с вложением файла
	/// </summary>
	public abstract class InfoForCreatingDocumentEdoWithAttachment : InfoForCreatingDocumentEdo
	{
		protected InfoForCreatingDocumentEdoWithAttachment(FileData fileData)
		{
			FileData = fileData;
		}
		
		/// <summary>
		/// Прикрепленный документ
		/// </summary>
		public FileData FileData { get; }
	}
}
