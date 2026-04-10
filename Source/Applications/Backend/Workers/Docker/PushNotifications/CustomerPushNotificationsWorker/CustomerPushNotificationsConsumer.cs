using CustomerPushNotifications.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CustomerNotificationsWorker
{

	public class CustomerPushNotificationsConsumer : IConsumer<CustomerNotificationIntegrationEvent>
	{
		private readonly ILogger<CustomerPushNotificationsConsumer> _logger;

		public CustomerPushNotificationsConsumer(ILogger<CustomerPushNotificationsConsumer> logger)
		{
			_logger = logger;
		}

		public Task Consume(ConsumeContext<CustomerNotificationIntegrationEvent> context)
		{
			var message = context.Message;

			_logger.LogInformation(
				"Получено push-уведомление из outbox. " +
				"UserId: {UserId}, Title: {Title}, Body: {Body}",
				message.Data.CounterpartyErpId);


			return Task.CompletedTask;
		}
	}
}
