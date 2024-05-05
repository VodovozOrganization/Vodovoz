using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public interface IDeliveryPriceCalculator
	{
		decimal CalculateDeliveryPrice(IUnitOfWork unitOfWork, Order order);
		decimal CalculateDeliveryPrice(OnlineOrder onlineOrder);
	}
}
