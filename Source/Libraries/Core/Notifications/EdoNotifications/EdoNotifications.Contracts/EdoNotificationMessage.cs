using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TransactionalOutbox.Contracts;
using TransactionalOutbox.Serialization;

namespace EdoNotifications.Contracts
{
	/// <summary>
	/// ЭДО-уведомление. Содержит информацию о типе уведомления и динамический набор параметров шаблона.
	/// </summary>
	public class EdoNotificationMessage : IIdempotentOutboxMessage
	{
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

			string finalHash = ComputeMd5Hash(stringToHash);

			return $"Event={nameof(EdoNotificationMessage)}:Hash={finalHash}";
		}

		private static string ComputeMd5Hash(string input)
		{
			using(var md5 = MD5.Create())
			{
				byte[] inputBytes = Encoding.UTF8.GetBytes(input);
				byte[] hashBytes = md5.ComputeHash(inputBytes);

				return BitConverter.ToString(hashBytes).Replace("-", "");
			}
		}
	}
}
