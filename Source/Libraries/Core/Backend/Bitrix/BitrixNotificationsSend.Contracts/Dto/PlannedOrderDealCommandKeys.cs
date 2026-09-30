namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Префиксы ключей команд пакетных запросов Битрикс24 по сделкам планируемых заказов
	/// </summary>
	public static class PlannedOrderDealCommandKeys
	{
		/// <summary>
		/// Префикс ключа команды создания сделки
		/// </summary>
		public const string CreateCommandKeyPrefix = "deal_";

		/// <summary>
		/// Префикс ключа команды обновления сделки
		/// </summary>
		public const string UpdateCommandKeyPrefix = "planned_order_deal_";
	}
}
