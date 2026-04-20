namespace TransactionalOutbox.Abstractions
{
	/// <summary>
	/// Провайдер настроек для событий, которые будут публиковаться через Outbox.
	/// </summary>
	/// <typeparam name="TEvent"></typeparam>
	public interface IOutBoxSettingsProvider<in TEvent>
	{
		/// <summary>
		/// Разрешены ли дубликаты сообщений
		/// </summary>
		/// <param name="eventType">Тип события</param>
		/// <returns></returns>
		bool IsDuplicateAllowed(TEvent notificationEvent);

		/// <summary>
		/// Отключена ли отправка для данного события
		/// </summary>
		/// <param name="eventType">Тип события</param>
		/// <returns></returns>
		bool IsDisabled(TEvent notificationEvent);
	}
}
