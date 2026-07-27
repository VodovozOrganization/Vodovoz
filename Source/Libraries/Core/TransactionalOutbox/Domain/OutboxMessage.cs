using System;
using TransactionalOutbox.Contracts;
using TransactionalOutbox.Extensions;

namespace TransactionalOutbox.Domain
{
	/// <summary>
	/// Сообщение аутбокса, которые будут отправлены после успешного коммита транзакции.
	/// </summary>
	public class OutboxMessage
	{
		/// <summary>
		/// Уникальный идентификатор сообщения
		/// </summary>
		public virtual Guid Guid { get; protected set; }

		/// <summary>
		/// Дата и время создания сообщения
		/// </summary>
		public virtual DateTime CreatedAt { get; protected set; }

		/// <summary>
		/// Тип сообщения
		/// </summary>
		public virtual string Type { get; protected set; }

		/// <summary>
		/// Payload в виде JSON строки. Будет десериализован в объект нужного типа при отправке сообщения.
		/// </summary>
		public virtual string Payload { get; protected set; }

		/// <summary>
		/// Дата и время отправки сообщения.
		/// </summary>
		public virtual DateTime? SentAt { get; protected set; }

		/// <summary>
		/// Количество попыток отправки сообщения.
		/// </summary>
		public virtual int Attempts { get; protected set; }

		/// <summary>
		/// Ошибка, которая произошла при последней попытке отправки сообщения.
		/// </summary>
		public virtual string Error { get; protected set; }

		/// <summary>
		/// Ключ для дедупликации сообщений. Определяет, является ли новое сообщение дубликатом уже существующего сообщения.
		/// </summary>
		public virtual string DeduplicationKey { get; protected set; }

		protected OutboxMessage() { }

		/// <summary>
		/// Инициализирует новый экземпляр класса OutboxMessage с указанным доменным событием и полезной нагрузкой.
		/// </summary>
		/// <param name="domainEvent">Доменное событие, связанное сообщением аутбокса. Используется для генерации ключа дедупликации.</param>
		/// <param name="payload">Объект, который будет сохранен в сообщениеи аутбокса. Представляет данные, которые будут сериализованы и отправлены.</param>
		/// <exception cref="ArgumentNullException">Выбрасывается, если domainEvent или payload равны null.</exception>
		public OutboxMessage(IIdempotentOutboxMessage domainEvent, object payload)
		{
			if(domainEvent == null)
			{
				throw new ArgumentNullException(nameof(domainEvent));
			}

			if(payload == null)
			{
				throw new ArgumentNullException(nameof(payload));
			}

			CreatedAt = DateTime.UtcNow;
			DeduplicationKey = domainEvent.GetDeduplicationKey();

			SavePayload(payload);
		}

		public OutboxMessage(IIdempotentOutboxMessage payload)
		{
			if(payload == null)
			{
				throw new ArgumentNullException(nameof(payload));
			}

			CreatedAt = DateTime.UtcNow;
			DeduplicationKey = payload.GetDeduplicationKey();

			SavePayload(payload);
		}

		public OutboxMessage(object payload, string deduplicationKey)
		{
			if(payload == null)
			{
				throw new ArgumentNullException(nameof(payload));
			}

			if(string.IsNullOrWhiteSpace(deduplicationKey))
			{
				throw new ArgumentException("DeduplicationKey is required", nameof(deduplicationKey));
			}

			CreatedAt = DateTime.UtcNow;
			DeduplicationKey = deduplicationKey.Trim();

			SavePayload(payload);
		}

		public OutboxMessage(object payload)
		{
			if(payload == null)
			{
				throw new ArgumentNullException(nameof(payload));
			}

			CreatedAt = DateTime.UtcNow;

			SavePayload(payload);
		}

		public virtual void MarkAsSent()
		{
			SentAt = DateTime.UtcNow;
		}

		public virtual void IncrementAttempts(string error = null)
		{
			Attempts++;

			if(!string.IsNullOrEmpty(error))
			{
				Error = error;
			}
		}

		/// <summary>
		/// Сохраняет объект в сообщении аутбокса, сериализуя его в JSON строку и устанавливая тип сообщения.
		/// </summary>
		public virtual void SavePayload(object payload)
		{
			if(payload == null)
			{
				throw new ArgumentNullException(nameof(payload));
			}

			Type = payload.GetType().FullName;
			Payload = payload.SerializeForOutbox();
		}

		public static OutboxMessage Create(object payload) =>
			new OutboxMessage(payload);

	}
}
