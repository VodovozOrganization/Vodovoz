using System.Threading;
using System.Threading.Tasks;

namespace Edo.Problem.Routine.Services.ReceiptFailedProblem
{
	/// <summary>
	/// Процессор переобработки отдельного фискального чека со статусом Failed
	/// </summary>
	public interface IReceiptFailedDocumentReprocessingProcessor
	{
		/// <summary>
		/// Выполняет проверку и повторную обработку фискального документа по его идентификатору в изолированной области видимости (Scope/UoW)
		/// </summary>
		/// <param name="fiscalDocumentId">Идентификатор фискального документа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат обработки отдельного документа</returns>
		Task<FiscalDocumentReprocessingResult> ProcessDocumentAsync(int fiscalDocumentId, CancellationToken cancellationToken);
	}
}
