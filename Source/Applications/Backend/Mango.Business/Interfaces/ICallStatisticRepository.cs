using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mango.Domain.Entity;

namespace Mango.Business.Interfaces
{
	public interface ICallStatisticRepository
	{
		Task<IEnumerable<CallEntity>> GetCallEntitiesAsync(
			DateTime startDate, 
			DateTime endDate,
			CancellationToken cancellationToken
			);
		
		Task InsertBatchAsync(
			IReadOnlyCollection<CallEntity> records,
			CancellationToken cancellationToken
			);

	}
}
