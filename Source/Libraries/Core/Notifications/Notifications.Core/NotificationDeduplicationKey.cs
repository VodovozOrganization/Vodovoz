using System;

namespace Notifications.Core
{
	/// <summary>
	/// Ключ дедупликации отправленного уведомления (таблица notification_deduplication_keys).
	/// Пишется в той же транзакции, что и уведомление в outbox, хранится ограниченное время,
	/// после чего удаляется службой очистки (NotificationDeduplicationKeyCleanupService в EdoNotificationsWorker).
	/// Ключ не уникален: параллельные транзакции не должны падать на дубликате.
	/// </summary>
	public class NotificationDeduplicationKey
	{
		protected NotificationDeduplicationKey() { }

		public NotificationDeduplicationKey(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
			{
				throw new ArgumentException("Не указан ключ дедупликации", nameof(key));
			}

			Key = key.Trim();
			CreatedAt = DateTime.UtcNow;
		}

		public virtual long Id { get; protected set; }

		public virtual string Key { get; protected set; }

		public virtual DateTime CreatedAt { get; protected set; }
	}
}
