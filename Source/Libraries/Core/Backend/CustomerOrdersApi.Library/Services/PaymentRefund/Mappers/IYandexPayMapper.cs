using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using CustomerOrdersApi.Library.Services.PaymentRefund.Models.YandexPay;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Mappers
{
	public interface IYandexPayMapper
	{
		/// <summary>
		/// Маппит из общего запроса на возврат в запрос для YandexPay API
		/// </summary>
		YandexPayRefundRequest MapToRefundRequest(RefundRequestDto request);

		/// <summary>
		/// Маппит из ответа YandexPay API в общий результат возврата
		/// </summary>
		RefundResultDto MapToRefundResult(YandexPayResult<YandexPayRefundResponse> yandexPayResponse);
	}
}
