using MassTransit;
using MassTransit.Context;
using MassTransit.Middleware;
using MassTransit.Serialization;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TransactionalOutbox.MassTransit.Domain;

namespace TransactionalOutbox.MassTransit
{
	/// <summary>
	/// Outbox одной шины в рамках одного <see cref="IUnitOfWork"/>: вместо отправки в транспорт
	/// сохраняет сообщения в таблицы outbox MassTransit через сессию NHibernate, то есть в текущую транзакцию UoW.
	/// Через него публикуют и десктоп, и бэкенд. Отправляет сообщения служба отправки MassTransit в воркерах бэкенда.
	/// Порт EntityFrameworkScopedBusContext.AddSend из MassTransit.EntityFrameworkCore: строки должны быть такими же,
	/// как пишет сам MassTransit.
	/// EF Core здесь не используется: это наша замена записи через EF Core из штатного outbox MassTransit.
	/// Штатное MassTransit: базовый класс OutboxSendContext, сериализация сообщения и заголовков.
	/// </summary>
	public sealed partial class UnitOfWorkOutboxContext : OutboxSendContext
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly string _busName;
		private readonly Dictionary<Guid, OutboxMessageRecord> _transactionMessages = new Dictionary<Guid, OutboxMessageRecord>();
		private OutboxStateRecord _outboxState;

		public UnitOfWorkOutboxContext(IUnitOfWork unitOfWork, string busName)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_busName = busName ?? throw new ArgumentNullException(nameof(busName));
		}

		/// <summary>
		/// Переопределение штатного метода MassTransit <see cref="OutboxSendContext.AddSend{T}"/>: в штатном outbox MassTransit
		/// его реализует EntityFrameworkScopedBusContext и пишет через EF Core, здесь — через NHibernate.
		/// Сохраняет одно сообщение в таблицы outbox в транзакции UoW.
		/// В нашем коде вызовов нет: метод реализует интерфейс MassTransit <see cref="OutboxSendContext"/>,
		/// и вызывает его сам MassTransit. При <c>Publish</c> через конечную точку из
		/// <see cref="UnitOfWorkOutbox{TBus}.GetPublishEndpoint"/> MassTransit готовит <paramref name="context"/>
		/// (сообщение, MessageId, заголовки, адрес) и вместо отправки в RabbitMQ передаёт его сюда
		/// </summary>
		public async Task AddSend<T>(SendContext<T> context)
			where T : class
		{
			if(!context.MessageId.HasValue)
			{
				throw new MessageException(typeof(T), "The SendContext MessageId must be present");
			}

			var outboxState = _outboxState ?? await StartOutboxAsync(context.CancellationToken).ConfigureAwait(false);

			var outboxMessage = CreateOutboxMessage(context, _busName, outboxState.OutboxId);

			await _unitOfWork.SaveAsync(outboxMessage, false, context.CancellationToken).ConfigureAwait(false);

			_transactionMessages[outboxMessage.MessageId] = outboxMessage;
		}

		/// <summary>
		/// Удаляет из текущей транзакции ранее добавленное сообщение
		/// </summary>
		/// <param name="messageId">Идентификатор сообщения</param>
		/// <returns>true, если сообщение было найдено и удалено</returns>
		public bool TryRemove(Guid messageId)
		{
			if(!_transactionMessages.TryGetValue(messageId, out var outboxMessage))
			{
				return false;
			}

			_unitOfWork.Delete(outboxMessage);
			_transactionMessages.Remove(messageId);

			return true;
		}

		private async Task<OutboxStateRecord> StartOutboxAsync(CancellationToken cancellationToken)
		{
			var outboxState = OutboxRecordFactory.CreateState(_busName);
			outboxState.OutboxId = NewId.NextGuid();
			outboxState.Created = DateTime.UtcNow;

			await _unitOfWork.SaveAsync(outboxState, false, cancellationToken).ConfigureAwait(false);

			// После коммита или отката следующая транзакция этого UoW должна получить новый OutboxId:
			// старое состояние может быть уже отправлено и удалено
			var transaction = _unitOfWork.Session.GetCurrentTransaction();

			if(transaction is null || !transaction.IsActive)
			{
				// Без активной транзакции не узнать о её завершении — у каждого сообщения своё состояние outbox
				return outboxState;
			}

			transaction.RegisterSynchronization(new ResetOnTransactionCompletion(this));

			_outboxState = outboxState;
			return outboxState;
		}

		private void Reset(bool committed)
		{
			if(!committed && _transactionMessages.Count > 0)
			{
				LogContext.Warning?.Log(
					"UoW закрыт без Commit, опубликованные сообщения не будут отправлены ({Count} шт.). "
					+ "Если это откат после ошибки — так и должно быть, иначе после публикации нужен Commit",
					_transactionMessages.Count);
			}

			_outboxState = null;
			_transactionMessages.Clear();
		}

		private static OutboxMessageRecord CreateOutboxMessage<T>(SendContext<T> context, string busName, Guid outboxId)
			where T : class
		{
			var serializer = SystemTextJsonMessageSerializer.Instance;
			var body = context.Serializer.GetMessageBody(context);
			var now = DateTime.UtcNow;

			var outboxMessage = OutboxRecordFactory.CreateMessage(busName);

			outboxMessage.OutboxId = outboxId;
			outboxMessage.MessageId = context.MessageId.Value;
			outboxMessage.ConversationId = context.ConversationId;
			outboxMessage.CorrelationId = context.CorrelationId;
			outboxMessage.InitiatorId = context.InitiatorId;
			outboxMessage.RequestId = context.RequestId;
			outboxMessage.SourceAddress = context.SourceAddress;
			outboxMessage.DestinationAddress = context.DestinationAddress;
			outboxMessage.ResponseAddress = context.ResponseAddress;
			outboxMessage.FaultAddress = context.FaultAddress;
			outboxMessage.SentTime = context.SentTime ?? now;
			outboxMessage.ContentType = context.ContentType?.ToString() ?? context.Serialization.DefaultContentType.ToString();
			outboxMessage.Body = body.GetString();
			outboxMessage.Headers = serializer.SerializeDictionary(context.Headers.GetAll());

			if(context.TimeToLive.HasValue)
			{
				outboxMessage.ExpirationTime = now + context.TimeToLive;
			}

			if(context.Delay.HasValue)
			{
				outboxMessage.EnqueueTime = now + context.Delay;
			}

			if(context is TransportSendContext<T> transportSendContext)
			{
				var properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				transportSendContext.WritePropertiesTo(properties);
				outboxMessage.Properties = serializer.SerializeDictionary(properties);
			}

			return outboxMessage;
		}
	}
}
