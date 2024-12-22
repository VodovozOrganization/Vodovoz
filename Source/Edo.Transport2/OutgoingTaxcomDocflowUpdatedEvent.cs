using System;

namespace Edo.Transport2
{
	/// <summary>
	/// Событие обновления статуса документооборота в Такском
	/// </summary>
	public class OutgoingTaxcomDocflowUpdatedEvent
	{
		public const string Event = "OutgoingTaxcomDocflowUpdated";
		/// <summary>
		/// Код кабинета в ЭДО
		/// </summary>
		public string EdoAccount { get; set; }
		/// <summary>
		/// Id главного документа
		/// </summary>
		public string MainDocumentId { get; set; }
		/// <summary>
		/// Id документооборота
		/// </summary>
		public Guid? DocFlowId { get; set; }
		/// <summary>
		/// Статус документооборота
		/// </summary>
		public int Status { get; set; }
		/// <summary>
		/// Время изменения состояния
		/// </summary>
		public DateTime StatusChangeDateTime { get; set; }
		/// <summary>
		/// Описание ошибки
		/// </summary>
		public string ErrorDescription { get; set; }
	}
}
