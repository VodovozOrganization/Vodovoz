using System.Threading;
using System.Threading.Tasks;
using YandexPayApi.Library.Models;
using YandexPayApi.Library.Requests;
using YandexPayApi.Library.Responses;

namespace YandexPayApi.Client
{
	/// <summary>
	/// API клиент для интеграции с YandexPay
	/// </summary>
	public interface IYandexPayApiClient
	{
		/// <summary>
		/// Получает информацию о заказе по его идентификатору
		/// </summary>
		/// <param name="orderId">Идентификатор заказа в системе YandexPay</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>
		/// Объект результата <see cref="YandexPayResult{T}"/> с данными о заказе <see cref="YandexPayOrderResponse"/>
		/// </returns>
		Task<YandexPayResult<YandexPayOrderResponse>> GetOrderAsync(string orderId, CancellationToken cancellationToken);

		/// <summary>
		/// Выполняет возврат средств по заказу
		/// </summary>
		/// <param name="request">Запрос на возврат средств <see cref="YandexPayRefundRequest"/></param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>
		/// Объект результата <see cref="YandexPayResult{T}"/> с данными о возврате <see cref="YandexPayRefundResponse"/>
		/// </returns>
		Task<YandexPayResult<YandexPayRefundResponse>> RefundAsync(YandexPayRefundRequest request, CancellationToken cancellationToken);
	}
}
