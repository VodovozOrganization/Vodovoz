using System.Threading;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Services.PaymentRefund.Models.CloudPayments
{
	public record CloudPaymentsRefundRequest(OnlineOrder OnlineOrder, string ExternalOrderId, decimal Amount, string TransactionId, CancellationToken CancellationToken);
}
