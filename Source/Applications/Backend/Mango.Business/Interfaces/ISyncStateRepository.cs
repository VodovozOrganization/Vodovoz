using System;
using System.Threading;
using System.Threading.Tasks;
using Mango.Domain.Entity;

namespace Mango.Business.Interfaces
{
	public interface ISyncStateRepository
	{
		/// <summary>
		/// Получить дату последней синхронизации звонков
		/// </summary>
		/// <param name="source"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<SyncStateEntity> GetAsync(
			string source, 
			CancellationToken cancellationToken);

		/// <summary>
		/// Сохранить дату синхронизации звонков
		/// </summary>
		/// <param name="source"></param>
		/// <param name="lastProcessedDate"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task SaveAsync(
			string source,
			DateTime lastProcessedDate,
			CancellationToken cancellationToken);
	}
}
