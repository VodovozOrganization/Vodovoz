using CustomerOrdersApi.Library.V6.Dto.Orders.CancelOrder;
using YooKassaApi.Library.Models;
using YooKassaApi.Library.Requests;
using YooKassaApi.Library.Responses;

namespace CustomerOrdersApi.Library.V6.Services.PaymentRefund.Mappers
{
	public interface IYooKassaMapper
	{
		/// <summary>
		/// Маппит из общего запроса на возврат в запрос для ЮKassa API
		/// </summary>
		/// <param name="request">Запрос на возврат ДС</param>
		/// <returns>Запрос на возврат ДС для ЮKassa API</returns>
		YooKassaRefundRequest MapToRefundRequest(RefundRequestDto request);

		/// <summary>
		/// Маппит из ответа ЮKassa API в общий результат возврата
		/// </summary>
		/// <param name="yooKassaResponse">Ответ на возврат ДС от ЮKassa API</param>
		/// <returns>Ответ на возврат ДС</returns>
		RefundResultDto MapToRefundResult(YooKassaResult<YooKassaRefundResponse> yooKassaResponse);
	}
}
