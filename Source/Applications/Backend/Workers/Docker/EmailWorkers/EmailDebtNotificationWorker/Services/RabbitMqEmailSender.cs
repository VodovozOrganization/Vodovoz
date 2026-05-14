using MassTransit;
using RabbitMQ.MailSending;
using System.Threading;
using System.Threading.Tasks;

namespace EmailDebtNotificationWorker.Services
{
	public class RabbitMqEmailSender : IEmailSender
	{
		private readonly IBus _bus;

		public RabbitMqEmailSender(IBus bus)
		{
			_bus = bus;
		}

		public Task Send(SendEmailMessage message, CancellationToken cancellationToken)
		{
			return _bus.Publish(message, cancellationToken);
		}
	}
}
