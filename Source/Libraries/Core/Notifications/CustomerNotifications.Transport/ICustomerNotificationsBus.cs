using MassTransit;

namespace CustomerNotifications.Transport
{
	/// <summary>
	/// Маркерный интерфейс для шины MassTransit, используемой для отправки пуш-уведомлений клиентам.
	/// </summary>
	public interface ICustomerNotificationsBus : IBus
	{
	}
}
