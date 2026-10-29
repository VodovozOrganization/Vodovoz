using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services.CodePoolMissingProblem
{
	public interface ICodePoolMissingProblemService
	{
		/// <summary>
		/// Попытка возобновить задачу ЭДО
		/// </summary>
		/// <param name="edoTask">ЭДО задача</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат попытки возобновления задачи</returns>
		Task TryResumeTaskAsync(OrderEdoTask edoTask, CancellationToken cancellationToken);

		/// <summary>
		/// Обработка задач ЭДО с проблемой отсутствия кода из пула
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат обработки задач</returns>
		Task ProcessProblemTasks(CancellationToken cancellationToken);
	}
}
