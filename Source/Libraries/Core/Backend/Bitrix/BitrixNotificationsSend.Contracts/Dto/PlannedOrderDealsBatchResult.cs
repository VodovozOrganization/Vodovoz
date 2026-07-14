using System.Collections.Generic;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Разобранный результат пакетного создания сделок по плановым заказам в Битрикс24
	/// </summary>
	public class PlannedOrderDealsBatchResult
	{
		/// <summary>
		/// Ключи команд, по которым сделки успешно созданы
		/// </summary>
		public IList<string> CreatedDealKeys { get; set; } = new List<string>();

		/// <summary>
		/// Ошибки создания сделок по отдельным командам пакета
		/// </summary>
		public IList<PlannedOrderDealCreationError> Errors { get; set; } = new List<PlannedOrderDealCreationError>();
	}
}
