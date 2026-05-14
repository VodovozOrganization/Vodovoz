using EmailDebtNotificationWorker.DTO;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmailDebtNotificationWorker.Builders
{
	/// <summary>
	/// Билдер писем для уведомления клиенту о закрытии поставок
	/// </summary>
	public interface IClientClosingDeliveriesEmailBuilder
	{
		Task<IReadOnlyList<SendEmailMessage>> Build(
			IUnitOfWork uow,
			IReadOnlyCollection<OrderWithoutShipmentForDebtNotificationInfo> notificationInfos,
			CancellationToken cancellationToken);

	}
}
