using System.Threading.Tasks;

namespace Vodovoz.NotificationSenders
{
	/// <summary>
	/// Отправка уведомлений об изменении времени ожидания
	/// </summary>
	public interface IWaitingTimeChangedNotificationSender
	{
		/// <summary>
		/// Отправить уведомление об изменении времени ожидания
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <returns></returns>
		Task NotifyOfWaitingTimeChanged(int orderId);
	}
}
