using CustomerOrdersApi.Library.V5.Dto.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Orders
{
	public interface IOnlineOrderTemplateFromOnlineOrderValidator
	{
		Result Validate(OnlineOrder onlineOrder, CreatingOrderTemplate creatingTemplate);
	}
}
