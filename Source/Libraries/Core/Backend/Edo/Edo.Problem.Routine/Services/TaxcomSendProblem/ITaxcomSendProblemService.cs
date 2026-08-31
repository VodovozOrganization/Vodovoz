using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services.TaxcomSendProblem
{
	public interface ITaxcomSendProblemService
	{
		/// <summary>
		/// Попытка возобновить задачу ЭДО
		/// </summary>
		/// <param name="edoTask">Задача ЭДО</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>ЭДО задача</returns>
		Task TryResumeTaskAsync(OrderEdoTask edoTask, CancellationToken cancellationToken);

		/// <summary>
		/// Обработка задач ЭДО с проблемой отправки в Такском
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>ЭДО задача</returns>
		Task ProcessProblemTasks(CancellationToken cancellationToken);
	}
}
