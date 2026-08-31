using CloudPaymentsApi.Library.Models;
using CloudPaymentsApi.Library.Requests;
using CloudPaymentsApi.Library.Responses;
using CustomerOrdersApi.Library.V7.Dto.Orders.CancelOrder;

namespace CustomerOrdersApi.Library.V7.Services.PaymentRefund.Mappers
{
	public interface ICloudPaymentsMapper
	{
		/// <summary>
		/// Маппит из общего запроса на возврат в запрос для CloudPayments API
		/// </summary>
		/// <param name="request">Запрос на возврат ДС</param>
		/// <returns>Запрос на возврат ДС для CloudPayments</returns>
		CloudPaymentsRefundRequest MapToRefundRequest(RefundRequestDto request);

		/// <summary>
		/// Маппит из ответа CloudPayments API в общий результат возврата
		/// </summary>
		/// <param name="cloudPaymentsResponse">Ответ на возврат ДС от CloudPayments</param>
		/// <returns>Ответ на возврат ДС</returns>
		RefundResultDto MapToRefundResult(CloudPaymentsResponse<CloudPaymentsRefundResult> cloudPaymentsResponse);
	}
}
