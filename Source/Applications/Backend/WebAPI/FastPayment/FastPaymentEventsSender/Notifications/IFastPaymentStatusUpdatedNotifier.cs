using System.Threading.Tasks;
using Vodovoz.Domain.FastPayments;

namespace FastPaymentEventsSender.Notifications
{
	/// <summary>
	/// Обработчик уведомлений о смене статуса платежа
	/// </summary>
	public interface IFastPaymentStatusUpdatedNotifier
	{
		/// <summary>
		/// Проверка и отправка уведомления о смене статуса платежа
		/// </summary>
		/// <param name="event">Событие</param>
		/// <returns></returns>
		Task NotifyPaymentStatusChangeAsync(FastPaymentStatusUpdatedEvent @event);
	}
}
