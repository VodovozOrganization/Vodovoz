using System;
using System.Threading.Tasks;

namespace ResourceLocker.Library
{
	public interface IResourceLocker : IAsyncDisposable
	{
		Task ReleaseLockResourceAsync();
		Task<ResourceLockResult> TryLockResourceAsync(TimeSpan? lockTimeout = null);
	}
}
