using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mango.Domain.Entity;

namespace Mango.Business.Interfaces
{
	public interface ICallStatisticRepository
	{
		Task InsertBatchAsync(
			IReadOnlyCollection<CallEntity> records,
			CancellationToken cancellationToken);

	}
}
