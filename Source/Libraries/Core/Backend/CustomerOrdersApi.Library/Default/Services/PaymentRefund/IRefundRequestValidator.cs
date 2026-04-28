using CustomerOrdersApi.Library.V4.Dto.Orders.CancelOrder;

namespace CustomerOrdersApi.Library.Default.Services.PaymentRefund
{
	public interface IRefundRequestValidator
	{
		/// <summary>
		/// Проверяет обязательные параметры запроса
		/// </summary>
		RefundResultDto Validate(RefundRequestDto request);
	}
}
