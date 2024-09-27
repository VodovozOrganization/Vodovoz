using System;

namespace TaxcomEdo.Contracts.Counterparties
{
	/// <summary>
	/// Информация о статусе контакта
	/// </summary>
	public class EdoContactState
	{
		/// <summary>
		/// Статус
		/// </summary>
		public EdoContactStateCode Code { get; set; }
		/// <summary>
		/// Описание
		/// </summary>
		public string Description { get; set; }
		/// <summary>
		/// Время изменения состояния
		/// </summary>
		public DateTime Changed { get; set; }
	}
}
