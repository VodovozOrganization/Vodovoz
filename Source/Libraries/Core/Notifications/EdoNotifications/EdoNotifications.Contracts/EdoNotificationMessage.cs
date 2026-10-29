using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using Notifications.Core;

namespace EdoNotifications.Contracts
{
	/// <summary>
	/// ЭДО-уведомление. Содержит тип уведомления и динамический набор параметров шаблона.
	/// Тип — значение колонки edo_notification_settings.edo_notification_type, по нему воркер уведомлений
	/// находит настройку (класс-наследник EdoNotificationSetting). Создаётся через IEdoNotificationMessageFactory
	/// </summary>
	public class EdoNotificationMessage : IDeduplicatedNotification
	{
		/// <summary>
		/// Тип ЭДО уведомления — имя класса настройки, например CodeDuplicatedEdoNotificationSetting
		/// </summary>
		public string EdoNotificationType { get; }

		/// <summary>
		/// Параметры шаблона
		/// </summary>
		public Dictionary<string, string> TemplateParams { get; }

		/// <summary>
		/// Создаёт EdoNotificationMessageFactory (тип — имя класса настройки), а при получении — десериализатор
		/// </summary>
		[JsonConstructor]
		internal EdoNotificationMessage(
			string edoNotificationType,
			Dictionary<string, string> templateParams = null)
		{
			EdoNotificationType = edoNotificationType;
			TemplateParams = templateParams ?? new Dictionary<string, string>();
		}

		/// <summary>
		/// В одной транзакции схлопываются только одинаковые уведомления (тот же тип и параметры):
		/// разные уведомления одного типа, например по разным заказам, отправляются все
		/// </summary>
		public string GetAggregateKey() => GetDeduplicationKey();

		/// <summary>
		/// Одинаковый у уведомлений одного типа с одинаковыми параметрами (порядок параметров не важен).
		/// Параметры хэшируются: текст сводок длинный, а ключ хранится в notification_deduplication_keys
		/// </summary>
		public string GetDeduplicationKey()
		{
			var text = EdoNotificationType + "\n" + string.Join("\n", TemplateParams
				.OrderBy(x => x.Key, StringComparer.Ordinal)
				.Select(x => $"{x.Key}={x.Value}"));

			using(var md5 = MD5.Create())
			{
				var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
				return $"{EdoNotificationType}:{string.Concat(hash.Select(x => x.ToString("x2")))}";
			}
		}
	}
}
