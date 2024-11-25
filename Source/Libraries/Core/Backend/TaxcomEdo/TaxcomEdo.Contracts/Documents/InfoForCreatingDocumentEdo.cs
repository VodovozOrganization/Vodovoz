using System;

namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Информация о id главного документа
	/// </summary>
	public abstract class InfoForCreatingDocumentEdo
	{
		/// <summary>
		/// Id главного документа для идентификации контейнера
		/// </summary>
		public Guid MainDocumentId { get; set; }
	}
}
