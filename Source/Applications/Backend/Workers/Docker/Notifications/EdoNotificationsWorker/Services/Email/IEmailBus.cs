using MassTransit;

namespace EdoNotificationsWorker.Services.Email
{
	/// <summary>
	/// Вторая шина EdoNotificationsWorker для vhost почты (email_host): письма-уведомления пишутся в её таблицы outbox (email_*).
	/// После перехода на один vhost RabbitMQ удаляется: письма публикуются через IPublishEndpoint основной шины
	/// </summary>
	public interface IEmailBus : IBus
	{
	}
}
