namespace Vodovoz.FirebaseCloudMessaging.Client.Requests
{
	/// <summary>
	/// Различные уровни видимости уведомления.<br/>
	/// <see href="https://firebase.google.com/docs/reference/fcm/rest/v1/projects.messages?hl=ru#visibility"/>
	/// </summary>
	public enum Visibility
	{
		/// <summary>
		/// Если не указано, по умолчанию используется <see cref="PRIVATE"/> .
		/// </summary>
		VISIBILITY_UNSPECIFIED,
		/// <summary>
		/// Покажите это уведомление на всех экранах блокировки, но скройте конфиденциальную или личную информацию на защищенных экранах блокировки.
		/// </summary>
		PRIVATE,
		/// <summary>
		/// Показывать это уведомление полностью на всех экранах блокировки.
		/// </summary>
		PUBLIC,
		/// <summary>
		/// Не раскрывайте никакую часть этого уведомления на защищенном экране блокировки.
		/// </summary>
		SECRET
	}
}
