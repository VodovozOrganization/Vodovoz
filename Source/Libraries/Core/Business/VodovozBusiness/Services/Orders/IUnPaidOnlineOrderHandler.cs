using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors.Logistics;

namespace VodovozBusiness.Services.Orders
{
	public interface IUnPaidOnlineOrderHandler
	{
		Task TryMoveToManualProcessingWaitingForPaymentOnlineOrders();
		Result CanChangePaymentType(OnlineOrder onlineOrder);
		Result TryUpdateOrder(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			Vodovoz.Domain.Logistic.DeliverySchedule deliverySchedule,
			UpdateOnlineOrderFromChangeRequest data);
	}
}
