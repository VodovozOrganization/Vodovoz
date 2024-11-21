using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors;

namespace Vodovoz.Services.Orders
{
	public interface IOrderFromOnlineOrderValidator
	{
		Result ValidateOnlineOrder(IUnitOfWork uow, OnlineOrder onlineOrder);
	}
}
