using System.Threading.Tasks;

namespace Vodovoz.NotificationSenders
{
	/// <summary>
	/// Отправка уведомлений о статусе оплаты через SMS
	/// </summary>
	public interface ISmsPaymentStatusNotificationSender
	{
		/// <summary>
		/// Отправить уведомление об изменении статуса оплаты через SMS
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <returns></returns>
		Task NotifyOfSmsPaymentStatusChanged(int orderId);
	}
}
