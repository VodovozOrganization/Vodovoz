using System.Threading;
using System.Threading.Tasks;

namespace Edo.Problem.Routine.Services.ReceiptFailedProblem
{
	/// <summary>
	/// Координатор выполнения регламентной переобработки чеков с ошибками (предотвращает параллельные запуски)
	/// </summary>
	public interface IReceiptReprocessingLockCoordinator
	{
		/// <summary>
		/// Попытка захватить блокировку выполнения переобработки чеков
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>True, если блокировка успешно захвачена</returns>
		Task<bool> TryAcquireLockAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Освобождение ранее захваченной блокировки выполнения
		/// </summary>
		/// <returns>Task</returns>
		Task ReleaseLockAsync();
	}
}
