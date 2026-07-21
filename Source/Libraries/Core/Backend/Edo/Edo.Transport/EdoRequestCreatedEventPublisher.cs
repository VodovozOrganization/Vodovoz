using System;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Edo.Transport
{
	/// <summary>
	/// Publishes events indicating that customer EDO requests have been created.
	/// </summary>
	public class EdoRequestCreatedEventPublisher : IEdoRequestCreatedEventPublisher
	{
		private readonly ILogger<EdoRequestCreatedEventPublisher> _logger;
		private readonly IBus _bus;

		public EdoRequestCreatedEventPublisher(
			ILogger<EdoRequestCreatedEventPublisher> logger,
			IBus bus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
		}

		/// <inheritdoc />
		public async Task Publish(
			int requestId,
			string operation,
			CancellationToken cancellationToken = default)
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
				await _bus.Publish(new EdoRequestCreatedEvent { Id = requestId }, cancellationToken);
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
			}
		}
	}
}
