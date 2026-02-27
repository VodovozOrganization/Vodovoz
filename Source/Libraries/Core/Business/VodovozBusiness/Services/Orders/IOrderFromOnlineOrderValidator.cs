using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Services.Orders
{
	public interface IOrderFromOnlineOrderValidator
	{
		Result ValidateOnlineOrder(IUnitOfWork uow, OnlineOrder onlineOrder, bool checkPerformedOrders = false);
	}
}
