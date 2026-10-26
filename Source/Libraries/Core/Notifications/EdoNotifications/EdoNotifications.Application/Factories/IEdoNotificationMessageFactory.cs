using EdoNotifications.Contracts;
using Vodovoz.Core.Domain.Edo;

namespace EdoNotifications.Application.Factories
{
	/// <summary>
	/// Фабрика для создания ЭДО-уведомлений: подставляет тип по классу настройки
	/// и вычисляет ключ дедупликации одинаково для всех сообщений.
	/// </summary>
	public interface IEdoNotificationMessageFactory
	{
		/// <summary>
		/// Создаёт ЭДО-уведомление с динамическим набором параметров для шаблона
		/// и вычисленным ключом дедупликации.
		/// </summary>
		/// <typeparam name="TSetting">Класс настройки уведомления, например CodeDuplicatedEdoNotificationSetting.
		/// Как добавить новое уведомление — см. <see cref="EdoNotificationSetting"/></typeparam>
		/// <param name="templateParams">Параметры шаблона в виде пар ключ-значение</param>
		EdoNotificationMessage Create<TSetting>(params (string Key, string Value)[] templateParams)
			where TSetting : EdoNotificationSetting;
	}
}
