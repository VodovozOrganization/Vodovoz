using CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.HttpClients
{
	public interface IYandexPayHttpClient
	{
		/// <summary>
		/// Получить информацию о заказе по ID
		/// <param name="orderId">Номер заказа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// </summary>
		Task<YandexPayResult<YandexPayOrderResponse>> GetOrderAsync(string orderId, CancellationToken cancellationToken);

		/// <summary>
		/// Выполнить возврат средств по заказу
		/// </summary>
		/// <param name="request">Запрос на возврат средств</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<YandexPayResult<YandexPayRefundResponse>> RefundAsync(YandexPayRefundRequest request, CancellationToken cancellationToken);
	}
}
