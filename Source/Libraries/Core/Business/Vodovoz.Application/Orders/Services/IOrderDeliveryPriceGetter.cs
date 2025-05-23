using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Orders;
using Vodovoz.Results;

namespace Vodovoz.Application.Orders.Services
{
	public interface IOrderDeliveryPriceGetter
	{
		Result<decimal, Exception> GetDeliveryPrice(IUnitOfWork unitOfWork, Order order);
	}
}
