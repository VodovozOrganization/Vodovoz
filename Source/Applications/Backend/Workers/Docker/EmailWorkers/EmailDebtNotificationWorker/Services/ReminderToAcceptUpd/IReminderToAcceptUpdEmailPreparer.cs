using EmailDebtNotificationWorker.DTO;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;

namespace EmailDebtNotificationWorker.Services.ReminderToAcceptUpd
{
	/// <summary>
	/// Подготовка писем с напоминанием о необходимости принятия УПД
	/// </summary>
	public interface IReminderToAcceptUpdEmailPreparer
	{
		/// <summary>
		/// Подготовить письма с напоминанием о необходимости принятия УПД
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="timedOutDocFlowGrouppedNode">Сгруппированная информация по клиентам, по которым есть непринятый УПД в ЭДО</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Сообщения для отправки email</returns>
		Task<IReadOnlyList<SendEmailMessage>> PrepareReminderToAcceptUpdEmails(
			IUnitOfWork uow,
			IReadOnlyCollection<TimedOutDocFlowGrouppedNode> timedOutDocFlowGrouppedNode,
			CancellationToken cancellationToken);
	}
}
