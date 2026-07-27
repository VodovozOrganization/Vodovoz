using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TransactionalOutbox.Contracts;
using TransactionalOutbox.Serialization;
using VodovozInfrastructure.Cryptography;

namespace EdoNotifications.Contracts
{
	/// <summary>
	/// ЭДО-уведомление. Содержит информацию о типе уведомления и динамический набор параметров шаблона.
	/// </summary>
	public class EdoNotificationMessage : IIdempotentOutboxMessage
	{
		private readonly IMD5HexHashFromString _mD5HexHashFromString;

		public EdoNotificationMessage(IMD5HexHashFromString mD5HexHashFromString)
		{
			_mD5HexHashFromString = mD5HexHashFromString ?? throw new ArgumentNullException(nameof(mD5HexHashFromString));
		}
		/// <summary>
		/// Тип ЭДО уведомления
		/// </summary>
		public EdoNotificationType EdoNotificationType { get; }

		/// <summary>
		/// Параметры шаблона
		/// </summary>
		public Dictionary<string, string> TemplateParams { get; }

		[JsonConstructor]
		private EdoNotificationMessage(
			EdoNotificationType edoNotificationType,
			Dictionary<string, string> templateParams = null)
		{
			EdoNotificationType = edoNotificationType;
			TemplateParams = templateParams ?? new Dictionary<string, string>();
		}

		/// <summary>
		/// Cоздание уведомления с динамическим набором параметров для шаблона
		/// </summary>
		public static EdoNotificationMessage Create(
			EdoNotificationType edoNotificationType,
			params (string Key, string Value)[] templateParams)
		{
			var dict = new Dictionary<string, string>();

			foreach(var (key, value) in templateParams)
			{
				dict[key] = value;
			}

			return new EdoNotificationMessage(edoNotificationType, dict);
		}

		/// <summary>
		/// Оптимизация для сессии. Схлопывает (обновляет) сообщения одного типа в рамках одной транзакции.
		/// </summary>
		public int GetAggregateId()
		{
			return (int)EdoNotificationType;
		}

		/// <summary>
		/// Дедупликация для БД. Гарантирует уникальность комбинации Типа события и его Параметров.
		/// </summary>
		public string GetDeduplicationKey()
		{
			var sortedParams = TemplateParams
				.OrderBy(p => p.Key)
				.ToDictionary(p => p.Key, p => p.Value);

			string jsonString = JsonSerializer.Serialize(sortedParams, OutboxJsonSerializerOptions.Instance);

			string stringToHash = $"Type:{(int)EdoNotificationType};Params:{jsonString}";

			string finalHash = _mD5HexHashFromString.GetMD5HexHashFromString(stringToHash);

			return $"Event={nameof(EdoNotificationMessage)}:Hash={finalHash}";
		}
	}
}
