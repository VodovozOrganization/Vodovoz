using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Controllers
{
	public interface IPaymentFromBankClientController
	{
		void UpdateAllocatedSum(IUnitOfWork uow, Order order);
		void ReturnAllocatedSumToClientBalanceIfChangedPaymentTypeFromCashless(IUnitOfWork uow, Order order);
		void ReturnAllocatedSumToClientBalance(
			IUnitOfWork uow,
			Order order,
			RefundPaymentReason refundPaymentReason = RefundPaymentReason.OrderCancellation
		);
		void CancelRefundedPaymentIfOrderRevertFromUndelivery(IUnitOfWork uow, Order order, OrderStatus previousOrderStatus);
		void CancellPaymentWithAllocationsByUserRequest(IUnitOfWork uow, int paymentId, bool needUpdateOrderPaymentStatus = true);
	}
}
