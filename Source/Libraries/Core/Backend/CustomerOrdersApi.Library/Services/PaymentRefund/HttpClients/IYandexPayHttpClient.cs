using CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.HttpClients
{
	public interface IYandexPayHttpClient
	{
		/// <summary>
		/// Получить информацию о заказе по ID
		/// </summary>
		Task<YandexPayResult<YandexPayOrderResponse>> GetOrderAsync(string orderId);

		/// <summary>
		/// Выполнить возврат средств по заказу
		/// </summary>
		Task<YandexPayResult<YandexPayRefundResponse>> RefundAsync(YandexPayRefundRequest request);
	}
}
