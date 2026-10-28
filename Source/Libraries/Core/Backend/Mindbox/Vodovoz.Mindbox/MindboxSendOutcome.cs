namespace Vodovoz.Mindbox
{
	/// <summary>
	/// Результат отправки запроса в Mindbox
	/// </summary>
	public enum MindboxSendOutcome
	{
		/// <summary>
		/// Клиент создан или обновлён
		/// </summary>
		Success = 0,

		/// <summary>
		/// Ошибка валидации данных, повтор не поможет
		/// </summary>
		ValidationError = 1,

		/// <summary>
		/// Ошибка протокола/запроса, повтор не поможет
		/// </summary>
		ProtocolError = 2,

		/// <summary>
		/// Нет ключа или ключ неверный
		/// </summary>
		Unauthorized = 3,

		/// <summary>
		/// Неверный URL операции
		/// </summary>
		NotFound = 4,

		/// <summary>
		/// Превышен лимит вызовов операции
		/// </summary>
		RateLimited = 5,

		/// <summary>
		/// Mindbox временно недоступен, нужен повтор
		/// </summary>
		TransientError = 6
	}
}
