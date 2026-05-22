using CustomerOrders.Contracts.V5.Orders;
using CustomerOrders.Contracts.V5.Orders.Templates;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Orders
{
	public interface IOnlineOrderTemplateFromOnlineOrderValidator
	{
		Result Validate(OnlineOrder onlineOrder, CreatingOrderTemplate creatingTemplate);
	}
}
