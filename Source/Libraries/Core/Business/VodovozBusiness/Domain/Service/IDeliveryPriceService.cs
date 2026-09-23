using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using VodovozBusiness.Domain.Orders.Delivery;

namespace VodovozBusiness.Domain.Service
{
	public interface IDeliveryPriceService
	{
		Result<decimal> GetDeliveryPrice(IUnitOfWork uow, IFreeDeliveryPrice saleSource);
	}
}
