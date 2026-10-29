using MassTransit;

namespace Notifications.Infrastructure
{
	/// <summary>
	/// Экземпляр второй шины <see cref="INotificationsBus"/> для DI (так MassTransit регистрирует дополнительные шины).
	/// Наследник штатного класса MassTransit BusInstance&lt;TBus&gt;, ничего в нём не переопределяет.
	/// После перехода на один vhost RabbitMQ удаляется вместе с <see cref="INotificationsBus"/>
	/// </summary>
	public class NotificationsBusInstance : BusInstance<INotificationsBus>, INotificationsBus
	{
		public NotificationsBusInstance(IBusControl busControl) : base(busControl)
		{
		}
	}
}
