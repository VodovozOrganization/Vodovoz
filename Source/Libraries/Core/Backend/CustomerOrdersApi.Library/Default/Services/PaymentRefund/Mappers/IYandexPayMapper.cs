using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using CustomerOrdersApi.Library.V4.Dto.Orders.CancelOrder;
using YandexPayApi.Library.Models;
using YandexPayApi.Library.Requests;
using YandexPayApi.Library.Responses;

namespace CustomerOrdersApi.Library.Default.Services.PaymentRefund.Mappers
{
	public interface IYandexPayMapper
	{
		/// <summary>
		/// Маппит из общего запроса на возврат в запрос для YandexPay API
		/// </summary>
		/// <param name="request">Запрос на возврат ДС</param>
		/// <param name="idempotenceKey">Ключ идемпотентности</param>
		/// <returns>Запрос на возврат ДС для YandexPay API</returns>
		YandexPayRefundRequest MapToRefundRequest(RefundRequestDto request, string idempotenceKey);

		/// <summary>
		/// Маппит из ответа YandexPay API в общий результат возврата
		/// </summary>
		/// <param name="yandexPayResponse">Ответ на возврат ДС от YandexPay API</param>
		/// <returns>Ответ на возврат ДС</returns>
		RefundResultDto MapToRefundResult(YandexPayResult<YandexPayRefundResponse> yandexPayResponse);
	}
}
