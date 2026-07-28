using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services
{
	/// <summary>
	/// Сервис формирования уведомлений о неустраненной проблеме контакта для отправки чека.
	/// </summary>
	public interface IReceiptContactProblemNotificationService
	{
		/// <summary>
		/// Пытается сохранить уведомление в transactional outbox.
		/// </summary>
		/// <param name="unitOfWork">Единица работы, в транзакции которой сохраняется уведомление.</param>
		/// <param name="receiptTask">Задача ЭДО на отправку чека.</param>
		/// <param name="problem">Активная проблема контакта.</param>
		/// <param name="retryCount">Количество выполненных попыток повторной обработки.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		/// <returns><see langword="true"/>, если уведомление сохранено в outbox.</returns>
		Task<bool> TryNotifyAsync(
			IUnitOfWork unitOfWork,
			ReceiptEdoTask receiptTask,
			EdoTaskProblem problem,
			int retryCount,
			CancellationToken cancellationToken);
	}
}
