using System;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using TransactionalOutbox.MassTransit;

namespace Edo.Transport
{
	/// <summary>
	/// Публикует события о создании заявок на отправку документов клиенту по ЭДО.
	/// </summary>
	public class EdoRequestCreatedEventPublisher : IEdoRequestCreatedEventPublisher
	{
		private readonly ILogger<EdoRequestCreatedEventPublisher> _logger;
		// UoW передаётся параметром: класс вызывается и с десктопа, где у диалогов свои UoW.
		// GetPublishEndpoint(uow) пишет сообщение в таблицы outbox в транзакции этого UoW.
		private readonly IUnitOfWorkOutbox<IBus> _outbox;

		public EdoRequestCreatedEventPublisher(
			ILogger<EdoRequestCreatedEventPublisher> logger,
			IUnitOfWorkOutbox<IBus> outbox)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_outbox = outbox ?? throw new ArgumentNullException(nameof(outbox));
		}

		/// <inheritdoc />
		public Task Publish(
			IUnitOfWork uow,
			int requestId,
			string operation,
			CancellationToken cancellationToken = default) =>
			Publish(_outbox.GetPublishEndpoint(uow), requestId, operation, cancellationToken);

		private async Task Publish(
			IPublishEndpoint publishEndpoint,
			int requestId,
			string operation,
			CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(operation))
			{
				throw new ArgumentException("Не указана операция, создавшая ЭДО-заявку.", nameof(operation));
			}

			_logger.LogInformation(
				"Публикуем событие создания ЭДО-заявки. RequestId: {RequestId}. Operation: {Operation}.",
				requestId,
				operation);

			try
			{
				await publishEndpoint.Publish(new EdoRequestCreatedEvent { Id = requestId }, cancellationToken);
				_logger.LogInformation(
					"Событие создания ЭДО-заявки опубликовано. RequestId: {RequestId}. Operation: {Operation}.",
					requestId,
					operation);
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Не удалось опубликовать событие создания ЭДО-заявки. RequestId: {RequestId}. Operation: {Operation}.",
					requestId,
					operation);
				throw;
			}
		}
	}
}
