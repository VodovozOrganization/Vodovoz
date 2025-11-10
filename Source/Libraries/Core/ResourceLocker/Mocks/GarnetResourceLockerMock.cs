using System;
using System.Threading.Tasks;

namespace ResourceLocker.Library.Mocks
{
	/// <summary>
	/// Заглушка для удобного тестирования
	/// </summary>
	public class GarnetResourceLockerMock : IResourceLocker
	{
		public async Task ReleaseLockResourceAsync()
		{
			await Task.CompletedTask;
		}

		public async Task<ResourceLockResult> TryLockResourceAsync(TimeSpan? lockTimeout = null)
		{
			return new ResourceLockResult
			{
				IsSuccess = true,
			};
		}

		public ValueTask DisposeAsync()
		{
			return new ValueTask();
		}
	}
}
