using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public interface IOrderDeliveryPriceGetter
	{
		decimal GetDeliveryPrice(IUnitOfWork unitOfWork, Order order);
	}
}
