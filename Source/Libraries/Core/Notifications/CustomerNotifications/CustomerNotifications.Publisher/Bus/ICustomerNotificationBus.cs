using MassTransit;

namespace CustomerNotifications.Publisher.Bus
{
	/// <summary>
	/// Маркерный интерфейс для отдельной шины,
	/// используемой для публикации уведомлений.
	/// </summary>
	public interface ICustomerNotificationBus: IBus { }
}
