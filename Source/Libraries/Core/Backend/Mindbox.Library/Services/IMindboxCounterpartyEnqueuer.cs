using System.Threading;
using System.Threading.Tasks;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Добавляет новых контрагентов в очередь отправки в Mindbox
	/// </summary>
	public interface IMindboxCounterpartyEnqueuer
	{
		/// <summary>
		/// Добавляет в очередь контрагентов, созданных после последнего обработанного
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Количество добавленных контрагентов</returns>
		Task<int> EnqueueNewCounterpartiesAsync(CancellationToken cancellationToken);

		/// <summary>
		/// Добавляет одного контрагента в очередь отправки в Mindbox
		/// </summary>
		/// <param name="counterpartyId">Идентификатор контрагента</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Задача постановки контрагента в очередь</returns>
		Task EnqueueCounterpartyAsync(int counterpartyId, CancellationToken cancellationToken);
	}
}
