using CustomerOrders.Contracts.V5.Carts;
using CustomerOrders.Contracts.V5.Orders.Templates;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Orders
{
	public interface IOnlineOrderTemplateFromOnlineOrderValidator
	{
		Result Validate(OnlineOrder onlineOrder, CreatingOrderTemplate creatingTemplate);
		Result Validate(IUnitOfWork uow, CheckUsersBasketRequest checkRequest);
	}
}
