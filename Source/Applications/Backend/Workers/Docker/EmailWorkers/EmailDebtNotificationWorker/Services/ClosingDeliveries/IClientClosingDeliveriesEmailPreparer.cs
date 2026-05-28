using EmailDebtNotificationWorker.DTO;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmailDebtNotificationWorker.Services.ClosingDeliveries
{
	/// <summary>
	/// Подготовка писем для уведомления клиентов о закрытии поставок
	/// </summary>
	public interface IClientClosingDeliveriesEmailPreparer
	{
		/// <summary>
		/// Подготовить письма для уведомления клиентов о закрытии поставок
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="notificationInfos"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<IReadOnlyList<SendEmailMessage>> PrepareClientEmails(
			IUnitOfWork uow,
			IReadOnlyCollection<OrderWithoutShipmentForDebtNotificationInfo> notificationInfos,
			CancellationToken cancellationToken);

	}
}
