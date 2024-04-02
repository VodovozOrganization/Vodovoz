using QS.DomainModel.UoW;
using System.Threading.Tasks;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public interface IOrderService
	{
		int PaidDeliveryNomenclatureId { get; }

		void CheckAndAddBottlesToReferrerByReferFriendPromo(IUnitOfWork uow, Order order, bool canChangeDiscountValue);
		int CreateAndAcceptOrder(RoboatsOrderArgs roboatsOrderArgs);
		Order CreateIncompleteOrder(RoboatsOrderArgs roboatsOrderArgs);
		decimal GetOrderPrice(RoboatsOrderArgs roboatsOrderArgs);
		void UpdateDeliveryCost(IUnitOfWork unitOfWork, Order order);
		int TryCreateOrderFromOnlineOrderAndAccept(IUnitOfWork uow, OnlineOrder onlineOrder);
		Order AcceptOrder(int orderId, int roboatsEmployee);
	}
}
