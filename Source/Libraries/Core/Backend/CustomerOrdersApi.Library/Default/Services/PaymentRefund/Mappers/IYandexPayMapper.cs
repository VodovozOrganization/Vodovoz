using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using YandexPayApi.Library.Models;
using YandexPayApi.Library.Requests;
using YandexPayApi.Library.Responses;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Mappers
{
	public interface IYandexPayMapper
	{
		/// <summary>
		/// Маппит из общего запроса на возврат в запрос для YandexPay API
		/// </summary>
		YandexPayRefundRequest MapToRefundRequest(RefundRequestDto request, string idempotenceKey);

		/// <summary>
		/// Маппит из ответа YandexPay API в общий результат возврата
		/// </summary>
		RefundResultDto MapToRefundResult(YandexPayResult<YandexPayRefundResponse> yandexPayResponse);
	}
}
