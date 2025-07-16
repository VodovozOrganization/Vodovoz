namespace Vodovoz.Settings.SecureCodes
{
	/// <summary>
	/// Настройки для кода авторизации
	/// </summary>
	public interface ISecureCodeSettings
	{
		/// <summary>
		/// Время до повторного запроса кода авторизации
		/// </summary>
		int TimeForNextCodeSeconds { get; }
		/// <summary>
		/// Время жизни кода авторизации
		/// </summary>
		int CodeLifetimeSeconds { get; }
	}
}
