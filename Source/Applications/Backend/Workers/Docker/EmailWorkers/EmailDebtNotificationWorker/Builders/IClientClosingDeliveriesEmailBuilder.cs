using EmailDebtNotificationWorker.DTO;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmailDebtNotificationWorker.Builders
{
	/// <summary>
	/// Билдер писем для уведомления клиентов о закрытии поставок
	/// </summary>
	public interface IClientClosingDeliveriesEmailBuilder
	{
		/// <summary>
		/// Создать письма для уведомления клиентов о закрытии поставок
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="notificationInfos"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<IReadOnlyList<SendEmailMessage>> Build(
			IUnitOfWork uow,
			IReadOnlyCollection<OrderWithoutShipmentForDebtNotificationInfo> notificationInfos,
			CancellationToken cancellationToken);

	}
}
