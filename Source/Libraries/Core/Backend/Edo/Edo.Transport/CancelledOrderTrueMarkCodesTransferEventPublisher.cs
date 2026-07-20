using System;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Edo.Transport
{
	/// <summary>
	/// Публикует события, необходимые после переноса кодов маркировки из отмененного заказа.
	/// </summary>
	public class CancelledOrderTrueMarkCodesTransferEventPublisher : ICancelledOrderTrueMarkCodesTransferEventPublisher
	{
		private readonly ILogger<CancelledOrderTrueMarkCodesTransferEventPublisher> _logger;
		private readonly IBus _bus;

		/// <summary>
		/// Создает экземпляр публикатора событий переноса кодов маркировки из отмененного заказа.
		/// </summary>
		/// <param name="logger">Логгер</param>
		/// <param name="bus">Шина сообщений</param>
		public CancelledOrderTrueMarkCodesTransferEventPublisher(
			ILogger<CancelledOrderTrueMarkCodesTransferEventPublisher> logger,
			IBus bus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
		}

		/// <inheritdoc />
		public async Task PublishEdoRequestCreated(int requestId, CancellationToken cancellationToken = default)
		{
			_logger.LogInformation(
				"Отправляем событие на создание ЭДО-заявки после переноса кодов маркировки, запрос: {RequestId}.",
				requestId);

			try
			{
				await _bus.Publish(new EdoRequestCreatedEvent { Id = requestId }, cancellationToken);
				_logger.LogInformation("Событие на создание ЭДО-заявки после переноса кодов маркировки отправлено успешно");
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при отправке события на создание ЭДО-заявки после переноса кодов маркировки. Id запроса: {RequestId}. Exception: {ExceptionMessage}",
					requestId,
					ex.Message);
			}
		}
	}
}
