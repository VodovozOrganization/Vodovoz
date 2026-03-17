using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Dto.Orders.CancelOrder
{
	public record RefundRequestDto(OnlineOrder OnlineOrder, string ExternalOrderId, decimal Amount, string TransactionId);
}
