using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public interface IOrderDeliveryPriceGetter
	{
		Result<decimal> GetDeliveryPrice(IUnitOfWork unitOfWork, Order order);
	}
}
