using CustomerOrdersApi.Library.V5.Dto.Orders.CancelOrder;

namespace CustomerOrdersApi.Library.V5.Services.PaymentRefund
{
	public interface IRefundRequestValidator
	{
		/// <summary>
		/// Проверяет обязательные параметры запроса
		/// </summary>
		RefundResultDto Validate(RefundRequestDto request);
	}
}
