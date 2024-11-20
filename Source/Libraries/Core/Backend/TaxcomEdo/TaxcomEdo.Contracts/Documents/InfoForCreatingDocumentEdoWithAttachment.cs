using System.Text.Json.Serialization;

namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Информация для отправки документа по ЭДО с вложением файла
	/// </summary>
	public abstract class InfoForCreatingDocumentEdoWithAttachment : InfoForCreatingDocumentEdo
	{
		/// <summary>
		/// Прикрепленный документ
		/// </summary>
		[JsonIgnore]
		public FileData FileData { get; set; }
	}
}
