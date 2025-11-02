using System;
using System.Collections.Generic;

namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Информация о документообороте Такскома
	/// </summary>
	public class EdoDocFlow
	{
		/// <summary>
		/// Id документооборота
		/// </summary>
		public Guid? Id { get; set; }
		/// <summary>
		/// Статус документооборота
		/// </summary>
		public string Status { get; set; }
		/// <summary>
		/// Внутренний статус документооборота
		/// </summary>
		public string InternalStatus { get; set; }
		/// <summary>
		/// Время изменения состояния
		/// </summary>
		public DateTime StatusChangeDateTime { get; set; }
		/// <summary>
		/// Документы
		/// </summary>
		public IEnumerable<EdoDocFlowDocument> Documents { get; set; }
		/// <summary>
		/// Описание ошибки
		/// </summary>
		public string ErrorDescription { get; set; }
	}
}
