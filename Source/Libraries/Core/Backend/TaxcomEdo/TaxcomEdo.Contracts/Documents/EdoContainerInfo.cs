using System;

namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Информация об ЭДО контейнере Такскома
	/// </summary>
	public class EdoContainerInfo
	{
		/// <summary>
		/// Id главного документа
		/// </summary>
		public string MainDocumentId { get; set; }
		/// <summary>
		/// Id документооборота
		/// </summary>
		public Guid? DocFlowId { get; set; }
		/// <summary>
		/// Получен документ клиентом или нет
		/// </summary>
		public bool Received { get; set; }
		/// <summary>
		/// Внутренний Id
		/// </summary>
		public Guid? InternalId { get; set; }
		/// <summary>
		/// Описание ошибки
		/// </summary>
		public string ErrorDescription { get; set; }
		/// <summary>
		/// Статус документооборота
		/// </summary>
		public string EdoDocFlowStatus { get; set; }
		/// <summary>
		/// Документы
		/// </summary>
		public byte[] Documents { get; set; }
	}
}
