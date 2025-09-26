using System;
using Microsoft.Extensions.Logging;
using ResourceLocker.Library.Providers;
using StackExchange.Redis;

namespace ResourceLocker.Library.Factories
{
	public class GarnetResourceLockerFactory : IResourceLockerFactory
	{
		private readonly IConnectionMultiplexer _connectionMultiplexer;
		private readonly IResourceLockerUniqueKeyProvider _resourceLockerUniqueKeyProvider;
		private readonly IResourceLockerValueProvider _resourceLockerValueProvider;
		private readonly ILogger<GarnetResourceLocker> _logger;

		public GarnetResourceLockerFactory(
			IConnectionMultiplexer connectionMultiplexer,
			IResourceLockerUniqueKeyProvider resourceLockerUniqueKeyProvider,
			IResourceLockerValueProvider resourceLockerValueProvider,
			ILogger<GarnetResourceLocker> logger
		)
		{
			_connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
			_resourceLockerUniqueKeyProvider =
				resourceLockerUniqueKeyProvider ?? throw new ArgumentNullException(nameof(resourceLockerUniqueKeyProvider));
			_resourceLockerValueProvider = resourceLockerValueProvider ?? throw new ArgumentNullException(nameof(resourceLockerValueProvider));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public IResourceLocker Create(string resourceKey)
		{
			if(string.IsNullOrWhiteSpace(resourceKey))
			{
				throw new ArgumentException("Имя ресурса не может быть пустым", nameof(resourceKey));
			}

			return new GarnetResourceLocker(
				_connectionMultiplexer,
				_resourceLockerUniqueKeyProvider,
				_resourceLockerValueProvider,
				_logger,
				resourceKey);
		}
	}
}
