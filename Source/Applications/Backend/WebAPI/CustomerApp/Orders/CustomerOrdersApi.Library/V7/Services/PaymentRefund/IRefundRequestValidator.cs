using CustomerOrdersApi.Library.V7.Dto.Orders.CancelOrder;

namespace CustomerOrdersApi.Library.V7.Services.PaymentRefund
{
	public interface IRefundRequestValidator
	{
		/// <summary>
		/// Проверяет обязательные параметры запроса
		/// </summary>
		RefundResultDto Validate(RefundRequestDto request);
	}
}
