using EmailSchedulerWorker.Handlers;
using MassTransit;

namespace EmailSchedulerWorker.Consumers
{
	/// <summary>
	/// Потребитель событий для обработки писем клиентов
	/// </summary>
	public class ProcessClientEmailConsumer : IConsumer<ProcessClientEmailEvent>
	{
		private readonly ILogger<ProcessClientEmailConsumer> _logger;
		private readonly IEmailSchedulerHandler _emailSchedulerHandler;

		public ProcessClientEmailConsumer(
			ILogger<ProcessClientEmailConsumer> logger,
			IEmailSchedulerHandler emailSchedulerHandler
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_emailSchedulerHandler = emailSchedulerHandler ?? throw new ArgumentNullException(nameof(emailSchedulerHandler));
		}

		public async Task Consume(ConsumeContext<ProcessClientEmailEvent> context)
		{
			try
			{
				_logger.LogInformation("Получено событие {event}, ClientId: {ClientId}, заказы: {OrderIds}",
					nameof(ProcessClientEmailEvent),
					context.Message.ClientId,
					string.Join(", ", context.Message.OrderIds));

				await _emailSchedulerHandler.HandleNew(context.Message.EmailMessage,
					context.Message.ClientId,
					context.Message.OrderIds,
					context.CancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError("Обнаружена ошибка при обработке события {event}, клиент: {ClientId} ошибка: {Error.Message}",
					nameof(ProcessClientEmailEvent),
					context.Message.ClientId,
					ex.Message);

				throw;
			}
		}
	}
}
