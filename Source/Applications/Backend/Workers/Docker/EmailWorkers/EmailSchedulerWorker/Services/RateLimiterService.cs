namespace EmailSchedulerWorker.Services
{
	/// <summary>
	/// Сервис для ограничения частоты отправки писем
	/// </summary>
	public class RateLimiterService : IRateLimiterService
	{
		private readonly ILogger<RateLimiterService> _logger;
		private const int _maxEmailsPerMinute = 10;

		private readonly Queue<DateTime> _sendTimestamps = new();
		private readonly SemaphoreSlim _lock = new(1, 1);

		public RateLimiterService(ILogger<RateLimiterService> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<bool> CanSendAsync()
		{
			await _lock.WaitAsync();
			try
			{
				CleanOldTimestamps();
				return _sendTimestamps.Count < _maxEmailsPerMinute;
			}
			finally
			{
				_lock.Release();
			}
		}

		public async Task RegisterSendAsync()
		{
			await _lock.WaitAsync();
			try
			{
				_sendTimestamps.Enqueue(DateTime.UtcNow);
				CleanOldTimestamps();

				_logger.LogDebug("Email sent. Current rate: {Count}/{Max}",
					_sendTimestamps.Count, _maxEmailsPerMinute);
			}
			finally
			{
				_lock.Release();
			}
		}

		public async Task DelayIfNeededAsync(CancellationToken cancellationToken)
		{
			while(true)
			{
				await _lock.WaitAsync(cancellationToken);
				try
				{
					CleanOldTimestamps();

					if(_sendTimestamps.Count < _maxEmailsPerMinute)
					{
						return;
					}

					var oldest = _sendTimestamps.Peek();
					var waitTime = oldest.AddMinutes(1) - DateTime.UtcNow;

					if(waitTime <= TimeSpan.Zero)
					{
						CleanOldTimestamps();
						continue;
					}

					_logger.LogDebug("Rate limit reached. Waiting {WaitTime}...", waitTime);

					_lock.Release();

					await Task.Delay(waitTime, cancellationToken);

					await _lock.WaitAsync(cancellationToken);
					CleanOldTimestamps();
				}
				finally
				{
					if(_lock.CurrentCount == 0)
					{
						_lock.Release();
					}
				}
			}
		}

		private void CleanOldTimestamps()
		{
			var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);

			while(_sendTimestamps.Count > 0 && _sendTimestamps.Peek() < oneMinuteAgo)
			{
				_sendTimestamps.Dequeue();
			}
		}
	}
}
