using System;
using System.Threading;
using System.Threading.Tasks;
using Mango.Domain.Entity;

namespace Mango.Business.Interfaces
{
	public interface ISyncStateRepository
	{
		Task<SyncStateEntity> GetAsync(
			string source, 
			CancellationToken cancellationToken);

		Task SaveAsync(
			string source,
			DateTime lastProcessedAtUtc,
			CancellationToken cancellationToken);
	}
}
