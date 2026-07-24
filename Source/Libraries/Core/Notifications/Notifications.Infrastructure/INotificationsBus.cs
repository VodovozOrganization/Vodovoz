using MassTransit;

namespace Notifications.Infrastructure
{
	/// <summary>
	/// Маркерный интерфейс для шины MassTransit, используемой для отправки уведомлений при наличии нескольких шин в проекте
	/// </summary>
	public interface INotificationsBus :IBus
	{
	}
}
