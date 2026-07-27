using EdoNotifications.Contracts;

namespace EdoNotifications.Application.Factories
{
	/// <summary>
	/// Фабрика для создания ЭДО-уведомлений. Вычисляет ключ дедупликации
	/// с использованием DI-сервиса хэширования, гарантируя единообразие
	/// его вычисления для всех создаваемых сообщений.
	/// </summary>
	public interface IEdoNotificationMessageFactory
	{
		/// <summary>
		/// Создаёт ЭДО-уведомление с динамическим набором параметров для шаблона
		/// и вычисленным ключом дедупликации.
		/// </summary>
		/// <param name="edoNotificationType">Тип ЭДО уведомления</param>
		/// <param name="templateParams">Параметры шаблона в виде пар ключ-значение</param>
		EdoNotificationMessage Create(
			EdoNotificationType edoNotificationType,
			params (string Key, string Value)[] templateParams);
	}
}
