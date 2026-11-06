using System.Threading;
using System.Threading.Tasks;

namespace Edo.Problem.Routine.Services.ReceiptFailedProblem
{
	/// <summary>
	/// Сервис мониторинга, проверки и автоматической переотправки чеков с ошибкой фискализации
	/// </summary>
	public interface IReceiptFailedReprocessingService
	{
		/// <summary>
		/// Выполняет процесс проверки и переотправки зависших чеков с ошибкой фискализации
		/// </summary>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Результат выполнения переобработки с количественными показателями</returns>
		Task<ReprocessingExecutionResult> ProcessFailedDocumentsAsync(CancellationToken cancellationToken);
	}
}
