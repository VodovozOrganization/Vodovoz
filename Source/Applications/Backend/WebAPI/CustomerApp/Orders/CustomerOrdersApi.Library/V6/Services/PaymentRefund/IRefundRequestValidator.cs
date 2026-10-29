using CustomerOrdersApi.Library.V6.Dto.Orders.CancelOrder;

namespace CustomerOrdersApi.Library.V6.Services.PaymentRefund
{
	public interface IRefundRequestValidator
	{
		/// <summary>
		/// Проверяет обязательные параметры запроса
		/// </summary>
		RefundResultDto Validate(RefundRequestDto request);
	}
}
