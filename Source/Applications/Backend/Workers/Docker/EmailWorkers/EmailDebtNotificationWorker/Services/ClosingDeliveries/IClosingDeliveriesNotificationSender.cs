using EmailDebtNotificationWorker.DTO;
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
		/// Отправка уведомлений о закрытии поставок. Работает в UoW своего DI-scope и коммитит его
		/// </summary>
		Task SendNotifications(
			IReadOnlyCollection<OrderWithoutShipmentForDebtNotificationInfo> notificationInfos,
			CancellationToken cancellationToken);
	}
}
