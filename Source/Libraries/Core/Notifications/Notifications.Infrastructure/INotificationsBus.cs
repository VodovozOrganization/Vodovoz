using MassTransit;

namespace Notifications.Infrastructure
{
	/// <summary>
	/// Маркерный интерфейс для шины MassTransit, используемой для отправки уведомлений при наличии нескольких шин в проекте.
	/// Вторая шина приложения для vhost'а уведомлений (только запись в outbox), см.
	/// <see cref="NotificationBusRegistrationExtensions.AddNotificationBusNotHosted"/>.
	/// После перехода на один vhost RabbitMQ удаляется: уведомления публикуются через IPublishEndpoint основной шины
	/// </summary>
	public interface INotificationsBus : IBus
	{
	}
}
