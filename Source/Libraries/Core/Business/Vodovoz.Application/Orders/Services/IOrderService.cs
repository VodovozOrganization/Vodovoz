using QS.DomainModel.UoW;
using System.Threading.Tasks;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public interface IOrderService
	{
		int PaidDeliveryNomenclatureId { get; }

		int CreateAndAcceptOrder(RoboatsOrderArgs roboatsOrderArgs);
		int CreateIncompleteOrder(RoboatsOrderArgs roboatsOrderArgs);
		Task<Order> CreateOrderWithPaymentByQrCode(string phone, RoboatsOrderArgs roboatsOrderArgs, bool needAcceptOrder);
		decimal GetOrderPrice(RoboatsOrderArgs roboatsOrderArgs);
		void UpdateDeliveryCost(IUnitOfWork unitOfWork, Order order);
	}
}
