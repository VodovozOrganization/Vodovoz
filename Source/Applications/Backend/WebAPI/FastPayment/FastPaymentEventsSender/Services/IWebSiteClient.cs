using System.Threading.Tasks;
using FastPaymentsApi.Contracts.Requests;

namespace FastPaymentEventsSender.Services
{
	/// <summary>
	/// Отправитель уведомлений для сайта
	/// </summary>
	public interface IWebSiteClient
	{
		/// <summary>
		/// Отправка уведомления
		/// </summary>
		/// <param name="notification">Уведомление</param>
		/// <returns></returns>
		Task<int> NotifyPaymentStatusChangedAsync(FastPaymentStatusChangeNotificationDto notification);
	}
}
