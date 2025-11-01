using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Orders
{
	public interface IOnlineOrderService
	{
		void NotifyClientOfOnlineOrderStatusChange(IUnitOfWork unitOfWork, OnlineOrder onlineOrder);
	}
}
