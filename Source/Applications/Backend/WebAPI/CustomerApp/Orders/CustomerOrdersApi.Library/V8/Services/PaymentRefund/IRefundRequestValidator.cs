using CustomerOrdersApi.Library.V8.Dto.Orders.CancelOrder;

namespace CustomerOrdersApi.Library.V8.Services.PaymentRefund
{
	public interface IRefundRequestValidator
	{
		/// <summary>
		/// Проверяет обязательные параметры запроса
		/// </summary>
		RefundResultDto Validate(RefundRequestDto request);
	}
}
