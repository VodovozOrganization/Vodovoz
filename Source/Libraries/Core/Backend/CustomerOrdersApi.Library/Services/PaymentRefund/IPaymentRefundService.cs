using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.Services.PaymentRefund
{
	public interface IPaymentRefundService
	{
		bool CanHandle(OnlinePaymentSource paymentSource);
		Task<RefundResultDto> ProcessRefundAsync(RefundRequestDto request);
	}
}
