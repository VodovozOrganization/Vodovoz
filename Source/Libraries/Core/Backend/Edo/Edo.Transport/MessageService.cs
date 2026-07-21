using System;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Edo.Transport
{
	/// <summary>
	/// Сервис для отправки сообщений в шину сообщений
	/// </summary>
	[Obsolete("Не используйте этот сервис. Разные сообщения отправляйте в отдельных сервисах " +
		"согласно их назначения, или в сервисах где создается документ соотвествующий сообщению")]
	public class MessageService
	{
		private readonly ILogger<MessageService> _logger;
		private readonly IBus _bus;

		public MessageService(ILogger<MessageService> logger, IBus bus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
		}

		/// <summary>
		/// Опубликовать событие о создании заявки по ЭДО
		/// </summary>
		/// <param name="requestId"></param>
		/// <returns></returns>
		public async Task PublishEdoRequestCreatedEvent(int requestId, CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("Отправляем событие на создание новой заявки по ЭДО, запрос: {RequestId}.", requestId);

			try
			{
				await _bus.Publish(new EdoRequestCreatedEvent { Id = requestId }, cancellationToken: cancellationToken);
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
	}
}
