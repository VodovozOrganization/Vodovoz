using System.Threading.Tasks;
using FastPaymentsApi.Contracts.Requests;

namespace FastPaymentEventsSender.Services
{
	/// <summary>
	/// Отправитель уведомлений для МП
	/// </summary>
	public interface IMobileAppNotifier
	{
		/// <summary>
		/// Отправка уведомления
		/// </summary>
		/// <param name="notification">Уведомление</param>
		/// <param name="url">Адрес, куда отправлять уведомление</param>
		/// <returns></returns>
		Task<int> NotifyPaymentStatusChangedAsync(FastPaymentStatusChangeNotificationDto notification, string url);
	}
}
