using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mango.Domain.Entity;

namespace Mango.Business.Interfaces
{
	public interface ICallStatisticRepository
	{
		/// <summary>
		/// Получить звонки
		/// </summary>
		/// <param name="startDate">С даты</param>
		/// <param name="endDate">По дату</param>
		/// <param name="cancellationToken"></param>
		/// <returns>Список сущностей</returns>
		Task<IEnumerable<CallEntity>> GetCallEntitiesAsync(
			DateTime startDate, 
			DateTime endDate,
			CancellationToken cancellationToken
			);
		
		/// <summary>
		/// Вставка списка сущностей
		/// </summary>
		/// <param name="records">Список сущностей р\о</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task InsertBatchAsync(
			IReadOnlyCollection<CallEntity> records,
			CancellationToken cancellationToken
			);

	}
}
