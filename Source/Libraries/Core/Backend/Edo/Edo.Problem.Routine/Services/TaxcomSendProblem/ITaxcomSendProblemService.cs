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
		/// <param name="orderDocumentId">Идентификатор документа заказа для ЭДО</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>ЭДО задача</returns>
		Task TryResumeOrderDocumentSendAsync(int orderDocumentId, CancellationToken cancellationToken);

		/// <summary>
		/// Обработка задач ЭДО с проблемой отправки в Такском
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>ЭДО задача</returns>
		Task ProcessProblemTasks(CancellationToken cancellationToken);
	}
}
