using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Edo.Transport
{
	public class MessageService
	{
		private readonly ILogger<MessageService> _logger;
		private readonly IBus _bus;

		public MessageService(ILogger<MessageService> logger, IBus bus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
		}
		
		public async Task PublishEdoRequestCreatedEvent(int requestId)
		{
			_logger.LogInformation("Отправляем событие на создание новой заявки по ЭДО, запрос: {RequestId}.", requestId);

			try
			{
				await _bus.Publish(new EdoRequestCreatedEvent { Id = requestId });
				_logger.LogInformation("Событие на создание новой заявки по ЭДО отправлено успешно");
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при отправке события на создание новой заявки по ЭДО. Id запроса: {RequestId}. Exception: {ExceptionMessage}",
					requestId,
					ex.Message);
			}
		}

		/// <summary>
		/// Опубликовать событие о создании неформальной заявки по ЭДО
		/// </summary>
		/// <param name="informalRequestId"></param>
		/// <returns></returns>
		public async Task PublishInformalEdoRequestCreatedEvent(int informalRequestId)
		{
			_logger.LogInformation("Отправляем событие на создание новой заявки по ЭДО, запрос: {RequestId}.", informalRequestId);

			try
			{
				await _bus.Publish(new InformalEdoRequestCreatedEvent { InformalRequestId = informalRequestId });
				_logger.LogInformation("Событие на создание новой заявки по ЭДО отправлено успешно");
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при отправке события на создание новой заявки по ЭДО. Id запроса: {RequestId}. Exception: {ExceptionMessage}",
					informalRequestId,
					ex.Message);
			}
		}
	}
}
