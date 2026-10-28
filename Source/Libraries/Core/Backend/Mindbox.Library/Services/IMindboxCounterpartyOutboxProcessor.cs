using System.Threading;
using System.Threading.Tasks;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Отправляет очередь контрагентов в Mindbox
	/// </summary>
	public interface IMindboxCounterpartyOutboxProcessor
	{
		/// <summary>
		/// Обрабатывает пачку сообщений очереди
		/// </summary>
		/// <returns>Количество обработанных сообщений</returns>
		Task<int> ProcessAsync(CancellationToken cancellationToken);
	}
}
