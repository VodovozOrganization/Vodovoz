using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public interface IOrderService
	{
		int PaidDeliveryNomenclatureId { get; }

		void CheckAndAddBottlesToReferrerByReferFriendPromo(IUnitOfWork uow, Order order, bool canChangeDiscountValue);
		int CreateAndAcceptOrder(RoboatsOrderArgs roboatsOrderArgs);
		(int OrderId, int AuthorId, OrderStatus OrderStatus) CreateIncompleteOrder(RoboatsOrderArgs roboatsOrderArgs);
		decimal GetOrderPrice(RoboatsOrderArgs roboatsOrderArgs);
		void UpdateDeliveryCost(IUnitOfWork unitOfWork, Order order);
		int TryCreateOrderFromOnlineOrderAndAccept(IUnitOfWork uow, OnlineOrder onlineOrder);
		(int OrderId, int AuthorId, OrderStatus OrderStatus) AcceptOrder(int orderId, int roboatsEmployeeId);
	}
}
