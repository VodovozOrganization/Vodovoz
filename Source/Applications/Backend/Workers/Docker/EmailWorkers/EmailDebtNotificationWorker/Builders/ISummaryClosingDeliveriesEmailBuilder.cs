using EmailDebtNotificationWorker.DTO;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmailDebtNotificationWorker.Builders
{
	/// <summary>
	/// Билдер общего информационного письма по всем клиентам о закрытии поставок
	/// </summary>
	public interface ISummaryClosingDeliveriesEmailBuilder
	{
		Task<IReadOnlyList<SendEmailMessage>> Build(
			IUnitOfWork uow,
			IReadOnlyCollection<OrderWithoutShipmentForDebtNotificationInfo> notificationInfos,
			CancellationToken cancellationToken);
	}
}
