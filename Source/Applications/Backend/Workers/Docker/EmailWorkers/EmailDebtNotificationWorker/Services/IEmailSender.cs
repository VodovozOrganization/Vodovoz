using RabbitMQ.MailSending;
using System.Threading;
using System.Threading.Tasks;

namespace EmailDebtNotificationWorker.Services
{
	/// <summary>
	/// Интерфейс для отправки писем
	/// </summary>
	public interface IEmailSender
	{
		/// <summary>
		/// Отправка письма
		/// </summary>
		Task Send(SendEmailMessage message, CancellationToken cancellationToken);
	}
}
