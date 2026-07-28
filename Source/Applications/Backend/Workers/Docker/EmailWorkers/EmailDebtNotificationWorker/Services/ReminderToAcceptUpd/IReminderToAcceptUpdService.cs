using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;

namespace EmailDebtNotificationWorker.Services.ReminderToAcceptUpd
{
	/// <summary>
	/// Сервис для отправки напоминаний о необходимости принятия УПД
	/// </summary>
	/// <param name="unitOfWork">UnitOfWork</param>
	/// <param name="timeoutDays">Количество дней до истечения срока принятия УПД</param>
	/// <param name="cancellationToken">Токен отмены</param>
	public interface IReminderToAcceptUpdService
	{
		Task RemindToAcceptUpd(IUnitOfWork unitOfWork, int timeoutDays, CancellationToken cancellationToken);
	}
}
