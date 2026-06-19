using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Orders
{
	public interface IUnPaidOnlineOrderHandler
	{
		Task TryMoveToManualProcessingWaitingForPaymentOnlineOrders(IUnitOfWork uow, CancellationToken cancellationToken);
		Result CanChangePaymentType(IUnitOfWork uow, OnlineOrder onlineOrder);
		Task<Result> TryUpdateOrderAsync(
			IUnitOfWork uow,
			IEnumerable<Order> orders,
			OnlineOrder onlineOrder,
			Vodovoz.Domain.Logistic.DeliverySchedule deliverySchedule,
			UpdateOnlineOrderFromChangeRequest data,
			CancellationToken cancellationToken);

		/// <summary>
		/// Отправляет уведомление клиенту о заказе, ожидающем оплаты.
		/// </summary>
		Task SendWaitingForPaymentNotificationsAsync(IUnitOfWork uow, CancellationToken cancellationToken); 
	}
}
