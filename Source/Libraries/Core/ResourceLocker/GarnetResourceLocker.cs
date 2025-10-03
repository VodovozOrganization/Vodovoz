using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ResourceLocker.Library.Providers;
using StackExchange.Redis;

namespace ResourceLocker.Library
{
	public class GarnetResourceLocker : IResourceLocker
	{
		private readonly IConnectionMultiplexer _connectionMultiplexer;
		private readonly ILogger<GarnetResourceLocker> _logger;
		private readonly IResourceLockerUniqueKeyProvider _resourceLockerUniqueKeyProvider;
		private readonly IResourceLockerValueProvider _resourceLockerValueProvider;
		private readonly TimeSpan _defaultLockDuration = TimeSpan.FromSeconds(10);
		private readonly string _resourceKey;
		private volatile CancellationTokenSource _renewalCts;
		private volatile Task _renewalTask;
		private int _renewalInProgress;

		public GarnetResourceLocker(
			IConnectionMultiplexer connectionMultiplexer,
			IResourceLockerUniqueKeyProvider resourceLockerUniqueKeyProvider,
			IResourceLockerValueProvider resourceLockerValueProvider,
			ILogger<GarnetResourceLocker> logger,
			string resourceKey
		)
		{
			if(string.IsNullOrEmpty(resourceKey))
			{
				throw new ArgumentNullException(nameof(resourceKey));
			}

			_connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
			_resourceLockerUniqueKeyProvider =
				resourceLockerUniqueKeyProvider ?? throw new ArgumentNullException(nameof(resourceLockerUniqueKeyProvider));
			_resourceLockerValueProvider = resourceLockerValueProvider ?? throw new ArgumentNullException(nameof(resourceLockerValueProvider));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_resourceKey = resourceKey;
		}

		private IDatabase Database => _connectionMultiplexer.GetDatabase();

		private string LockKey => _resourceLockerUniqueKeyProvider.GetResourceLockerUniqueKeyByResourceName(_resourceKey);

		private string CurrentUserLockValue => _resourceLockerValueProvider.GetResourceLockerValue();

		private async Task StartRenewal(TimeSpan ttl)
		{
			await StopRenewal().ConfigureAwait(false);

			if(Interlocked.Exchange(ref _renewalInProgress, 1) == 1)
			{
				return;
			}

			_renewalCts = new CancellationTokenSource();

			var cancellationToken = _renewalCts.Token;

			var renewalDuration = TimeSpan.FromTicks((long)(ttl.Ticks * 0.8));

			// Запускаем "Watchdog"
			_renewalTask = Task.Run(async () =>
				{
					try
					{
						while(!cancellationToken.IsCancellationRequested)
						{
							await Task.Delay(renewalDuration, cancellationToken).ConfigureAwait(false);

							var owner = await Database
								.StringGetAsync(LockKey)
								.ConfigureAwait(false);

							if(owner != CurrentUserLockValue)
							{
								await StopRenewal().ConfigureAwait(false);

								_logger.LogWarning($"{CurrentUserLockValue} Блокировка ресурса '{LockKey}' утрачена или захвачена другим пользователем.");

								break;
							}

							var extended = await Database
								.KeyExpireAsync(LockKey, ttl)
								.ConfigureAwait(false);

							if(extended)
							{
								_logger.LogInformation($"{CurrentUserLockValue} Продлена блокировка ресурса {LockKey}");
							}
							else
							{
								_logger.LogWarning($"{CurrentUserLockValue} Не удалось продлить блокировку ресурса {LockKey}");
							}
						}
					}
					catch(TaskCanceledException)
					{
					}
					catch(Exception ex)
					{
						_logger.LogError(ex, $"{CurrentUserLockValue} Ошибка при продлении блокировки ресурса {LockKey}");
					}
					finally
					{
						Interlocked.Exchange(ref _renewalInProgress, 0);
					}
				},
				cancellationToken);
		}

		private async Task StopRenewal()
		{
			var cts = Interlocked.Exchange(ref _renewalCts, null);

			if(cts == null || cts.IsCancellationRequested)
			{
				return;
			}

			cts.Cancel();

			try
			{
				if(_renewalTask != null)
				{
					await _renewalTask.ConfigureAwait(false);
				}
			}
			catch
			{
			}
			finally
			{
				cts.Dispose();
			}
		}

		public async Task<ResourceLockResult> TryLockResourceAsync(TimeSpan? ttl = null)
		{
			ttl = ttl ?? _defaultLockDuration;

			var ownerLockValue = await Database
				.StringGetAsync(LockKey)
				.ConfigureAwait(false);

			if(ownerLockValue == CurrentUserLockValue)
			{
				await Database.KeyExpireAsync(LockKey, ttl).ConfigureAwait(false);

				await StartRenewal(ttl.Value).ConfigureAwait(false);

				_logger.LogInformation($"Обновлена блокировка ресурса {LockKey}, пользователь: {CurrentUserLockValue}");

				return new ResourceLockResult
				{
					IsSuccess = true,
					OwnerLockValue = ownerLockValue.ToString()
				};
			}

			if(!ownerLockValue.IsNullOrEmpty)
			{
				var errorMessage = $"Ресурс {LockKey} уже заблокирован пользователем {ownerLockValue}";
				_logger.LogWarning(errorMessage);

				return new ResourceLockResult
				{
					IsSuccess = false,
					ErrorMessage = errorMessage,
					OwnerLockValue = ownerLockValue.ToString()
				};
			}

			try
			{
				var acquired = await Database
					.StringSetAsync(LockKey, CurrentUserLockValue, ttl, When.NotExists)
					.ConfigureAwait(false);

				if(acquired)
				{
					await StartRenewal(ttl.Value).ConfigureAwait(false);

					_logger.LogInformation($"Захвачена блокировка ресурса {LockKey}, пользователь: {CurrentUserLockValue}");

					return new ResourceLockResult
					{
						IsSuccess = true,
						OwnerLockValue = CurrentUserLockValue
					};
				}
			}
			catch(Exception ex)
			{
				var errorMessage = $"Ошибка при попытке захватить блокировку ресурса {LockKey} пользователем {CurrentUserLockValue}";

				_logger.LogError(ex, errorMessage);

				return new ResourceLockResult
				{
					IsSuccess = false,
					ErrorMessage = errorMessage
				};
			}

			return new ResourceLockResult
			{
				IsSuccess = false,
			};
		}

		public async Task ReleaseLockResourceAsync()
		{
			await StopRenewal().ConfigureAwait(false);

			const string atomicUnlockLuaScript = @"
                if redis.call('GET', KEYS[1]) == ARGV[1] then
                    return redis.call('DEL', KEYS[1])
                else
                    return 0
                end";

			try
			{
				var removed = (int)await Database
					.ScriptEvaluateAsync(atomicUnlockLuaScript, new RedisKey[] { LockKey }, new RedisValue[] { CurrentUserLockValue })
					.ConfigureAwait(false);

				if(removed == 1)
				{
					_logger.LogInformation($"{CurrentUserLockValue} Снята блокировка ресурса {LockKey}");
				}
				else
				{
					_logger.LogWarning($"{CurrentUserLockValue} Не удалось снять блокировку ресурса {LockKey}");
				}
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"{CurrentUserLockValue} Ошибка при попытке снять блокировку ресурса {LockKey}");
			}
		}

		public async ValueTask DisposeAsync()
		{
			await ReleaseLockResourceAsync().ConfigureAwait(false);
		}
	}
}
