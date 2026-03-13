using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using CustomerOrdersApi.Library.Services.PaymentRefund.Models.YooKassa;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Mappers
{
	public interface IYooKassaMapper
	{
		/// <summary>
		/// Маппит из общего запроса на возврат в запрос для ЮKassa API
		/// </summary>
		YooKassaRefundRequest MapToRefundRequest(RefundRequestDto request);

		/// <summary>
		/// Маппит из ответа ЮKassa API в общий результат возврата
		/// </summary>
		RefundResultDto MapToRefundResult(YooKassaResult<YooKassaRefundResponse> yooKassaResponse);
	}
}
