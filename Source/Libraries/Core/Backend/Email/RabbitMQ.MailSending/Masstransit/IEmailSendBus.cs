using MassTransit;

namespace RabbitMQ.EmailSending.Masstransit
{
	/// <summary>
	/// Вторая шина отправки писем (EmailSend.Library): потребитель SendEmailMessageConsumer на vhost pacs.
	/// После перехода на один vhost RabbitMQ удаляется вместе с дублем потребителя email.send_message.consumer
	/// </summary>
	public interface IEmailSendBus : IBus { }
}
