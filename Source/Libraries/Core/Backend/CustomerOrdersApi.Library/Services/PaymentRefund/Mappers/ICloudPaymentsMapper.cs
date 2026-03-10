using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using CustomerOrdersApi.Library.Services.PaymentRefund.Models.CloudPayments;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Mappers
{
	public interface ICloudPaymentsMapper
	{
		/// <summary>
		/// Маппит из общего запроса на возврат в запрос для CloudPayments API
		/// </summary>
		CloudPaymentsRefundRequest MapToRefundRequest(RefundRequestDto request);

		/// <summary>
		/// Маппит из ответа CloudPayments API в общий результат возврата
		/// </summary>
		RefundResultDto MapToRefundResult(CloudPaymentsResponse<CloudPaymentsRefundResult> cloudPaymentsResponse);
	}
}
