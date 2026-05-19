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
		/// <summary>
		/// Построить письмо с общей информацией по всем клиентам о закрытии поставок, у которых есть задолженность по оплате
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
