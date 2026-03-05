namespace Vodovoz.Core.Data.Repositories
{
	/// <summary>
	/// Режим поиска задач по ЭДО с истекшим таймаутом
	/// </summary>
	public enum TimedOutDocumentTasksSearchMode
	{
		/// <summary>
		/// Только зарегистрированные в ЧЗ клиенты
		/// </summary>
		OnlyTrueMarkRegisteredClients,
		/// <summary>
		/// Только не зарегистрированные в ЧЗ клиенты
		/// </summary>
		OnlyTrueMarkNotRegisteredClients
	}
}
