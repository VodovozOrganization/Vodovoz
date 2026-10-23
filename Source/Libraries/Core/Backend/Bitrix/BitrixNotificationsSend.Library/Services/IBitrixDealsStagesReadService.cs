using BitrixNotificationsSend.Contracts.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BitrixNotificationsSend.Library.Services
{
	/// <summary>
	/// Сервис пакетного чтения текущих стадий сделок из Битрикс24
	/// </summary>
	public interface IBitrixDealsStagesReadService
	{
		/// <summary>
		/// Чтение текущих стадий сделок из Битрикс24 пакетами
		/// </summary>
		/// <param name="dealIds">Id сделок в Битрикс24, для которых нужно получить стадии</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат чтения стадий сделок</returns>
		Task<BitrixDealsStagesResult> GetDealsStages(IEnumerable<long> dealIds, CancellationToken cancellationToken);
	}
}
