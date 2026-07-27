using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using TransactionalOutbox.Contracts;

namespace EdoNotifications.Contracts
{
	/// <summary>
	/// ЭДО-уведомление. Содержит информацию о типе уведомления и динамический набор параметров шаблона.
	/// </summary>
	public class EdoNotificationMessage : IIdempotentOutboxMessage
	{
		private readonly string _deduplicationKey;

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
		/// Конструктор для <see cref="EdoNotificationMessageFactory"/>.
		/// Принимает уже вычисленный ключ дедупликации, чтобы гарантировать
		/// единообразие его вычисления через DI-сервис хэширования.
		/// </summary>
		internal EdoNotificationMessage(
			EdoNotificationType edoNotificationType,
			Dictionary<string, string> templateParams,
			string deduplicationKey)
			: this(edoNotificationType, templateParams)
		{
			_deduplicationKey = deduplicationKey;
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
		public string GetDeduplicationKey() =>
			_deduplicationKey
			?? throw new InvalidOperationException(
				$"Дедупликационный ключ не был вычислен.");
	}
}
