namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Ошибка создания сделки по одной из команд пакетного запроса в Битрикс24
	/// </summary>
	public class PlannedOrderDealCreationError
	{
		/// <summary>
		/// Ключ команды в пакете, содержит id контрагента и точки доставки (или self для самовывоза)
		/// </summary>
		public string CommandKey { get; set; }

		/// <summary>
		/// Описание ошибки из ответа Битрикс24
		/// </summary>
		public string Message { get; set; }
	}
}
