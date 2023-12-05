using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	internal interface IOrderService
	{
		int PaidDeliveryNomenclatureId { get; }

		void UpdateDeliveryCost(IUnitOfWork unitOfWork, Order order);
	}
}