namespace DeliveryRulesService.V2.DTO
{
	/// <summary>
	/// Статус ответа на запрос правил доставки
	/// </summary>
	public enum DeliveryRulesResponseStatus
	{
		/// <summary>
		/// Успешное выполнение
		/// </summary>
		Ok,
		/// <summary>
		/// Правило не найдено
		/// </summary>
		RuleNotFound,
		/// <summary>
		/// Ошибка
		/// </summary>
		Error
	}
}
