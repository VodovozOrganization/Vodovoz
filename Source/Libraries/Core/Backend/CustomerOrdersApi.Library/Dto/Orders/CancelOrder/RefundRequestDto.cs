using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Dto.Orders.CancelOrder
{
	// Это же больше для CloudPayments, подумать над общим реквестом
	public record RefundRequestDto(OnlineOrder OnlineOrder, string ExternalOrderId, decimal Amount, string TransactionId);
}
