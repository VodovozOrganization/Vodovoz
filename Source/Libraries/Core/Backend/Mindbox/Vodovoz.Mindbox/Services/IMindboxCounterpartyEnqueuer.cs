using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Mindbox.Services
{
	/// <summary>
	/// Добавляет новых контрагентов в очередь отправки в Mindbox
	/// </summary>
	public interface IMindboxCounterpartyEnqueuer
	{
		/// <summary>
		/// Добавляет в очередь контрагентов, созданных после последнего обработанного
		/// </summary>
		/// <returns>Количество добавленных контрагентов</returns>
		Task<int> EnqueueNewCounterpartiesAsync(CancellationToken cancellationToken);
	}
}
