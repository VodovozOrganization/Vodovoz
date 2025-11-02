using System.Threading.Tasks;

namespace Vodovoz.NotificationSenders
{
	/// <summary>
	/// Отправка уведомлений о добавлении заказа с доставкой за час
	/// </summary>
	public interface IFastDeliveryOrderAddedNotificationSender
	{
		/// <summary>
		/// Отправить уведомление о добавлении заказа с доставкой за час в МЛ
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <returns></returns>
		Task NotifyOfFastDeliveryOrderAdded(int orderId);
	}
}
