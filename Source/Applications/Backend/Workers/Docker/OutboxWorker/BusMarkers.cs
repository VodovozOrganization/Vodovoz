using MassTransit;

namespace OutboxWorker
{
	/// <summary>Шина для vhost'а уведомлений</summary>
	public interface INotificationBus : IBus { }

	/// <summary>Шина для vhost'а ЭДО</summary>
	public interface IPacsBus : IBus { }
}
