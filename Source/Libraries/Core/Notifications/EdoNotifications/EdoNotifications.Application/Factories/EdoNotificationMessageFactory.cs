using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;

public class EdoNotificationMessageFactory : IEdoNotificationMessageFactory
{
	public EdoNotificationMessage Create<TSetting>(params (string Key, string Value)[] templateParams)
		where TSetting : EdoNotificationSetting
	{
		// Тип уведомления — имя класса настройки, оно же значение edo_notification_type
		var edoNotificationType = typeof(TSetting).Name;

		var parameters = new Dictionary<string, string>();

		foreach(var (key, value) in templateParams)
		{
			parameters[key] = value;
		}

		return new EdoNotificationMessage(edoNotificationType, parameters);
	}
}
