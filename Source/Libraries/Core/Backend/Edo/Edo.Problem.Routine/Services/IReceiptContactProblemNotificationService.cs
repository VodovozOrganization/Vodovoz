using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services
{
	public interface IReceiptContactProblemNotificationService
	{
		/// <summary>
		/// Отправить уведомление о нерешенной проблеме с контактом чека.
		/// </summary>
		/// <param name="receiptTask">Задача чека</param>
		/// <param name="problem">Проблема с контактом</param>
		/// <param name="retryCount">Количество выполненных попыток повторной обработки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task NotifyAsync(
			ReceiptEdoTask receiptTask,
			EdoTaskProblem problem,
			int retryCount,
			CancellationToken cancellationToken);
	}
}
