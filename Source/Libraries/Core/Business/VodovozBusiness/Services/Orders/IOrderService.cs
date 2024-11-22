using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Orders
{
	public interface IOrderService
	{
		int PaidDeliveryNomenclatureId { get; }

		void CheckAndAddBottlesToReferrerByReferFriendPromo(IUnitOfWork uow, Order order, bool canChangeDiscountValue);
		int CreateAndAcceptOrder(CreateOrderRequest roboatsOrderArgs);
		(int OrderId, int AuthorId, OrderStatus OrderStatus) CreateIncompleteOrder(CreateOrderRequest roboatsOrderArgs);
		decimal GetOrderPrice(CreateOrderRequest roboatsOrderArgs);
		void UpdateDeliveryCost(IUnitOfWork unitOfWork, Order order);
		int TryCreateOrderFromOnlineOrderAndAccept(IUnitOfWork uow, OnlineOrder onlineOrder);
		(int OrderId, int AuthorId, OrderStatus OrderStatus) AcceptOrder(int orderId, int roboatsEmployeeId);
		bool NeedResendByEdo(IUnitOfWork unitOfWork, Order entity);
		void AutoCancelAutoTransfer(IUnitOfWork uow, Order order);
	}
}
