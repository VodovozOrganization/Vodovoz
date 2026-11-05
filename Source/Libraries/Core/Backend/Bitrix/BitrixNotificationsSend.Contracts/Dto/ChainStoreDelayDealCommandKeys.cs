namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Префиксы ключей команд пакетных запросов Битрикс24 по сделкам отсрочки сети
	/// </summary>
	public static class ChainStoreDelayDealCommandKeys
	{
		/// <summary>
		/// Префикс ключа команды создания сделки
		/// </summary>
		public const string CreateCommandKeyPrefix = "chain_store_delay_deal_";

		/// <summary>
		/// Префикс ключа команды закрытия сделки
		/// </summary>
		public const string CloseCommandKeyPrefix = "chain_store_delay_close_";
	}
}
