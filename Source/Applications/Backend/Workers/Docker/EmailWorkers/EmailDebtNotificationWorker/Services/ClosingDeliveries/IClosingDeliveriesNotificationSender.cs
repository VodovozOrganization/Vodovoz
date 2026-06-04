using EmailDebtNotificationWorker.DTO;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmailDebtNotificationWorker.Services.ClosingDeliveries
{
	/// <summary>
	/// Сервис для отправки уведомлений о закрытии поставок
	/// </summary>
	public interface IClosingDeliveriesNotificationSender
	{
		/// <summary>
		/// Отправка уведомлений о закрытии поставок
		/// </summary>
		Task SendNotifications(
			IUnitOfWork uow,
			IReadOnlyCollection<OrderWithoutShipmentForDebtNotificationInfo> notificationInfos,
			CancellationToken cancellationToken);
	}
}
