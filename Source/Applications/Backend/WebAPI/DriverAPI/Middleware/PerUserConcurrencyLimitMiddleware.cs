using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DriverAPI.Middleware
{
	/// <summary>
	/// Ограничивает количество одновременно обрабатываемых запросов от одного пользователя.
	/// Лишние запросы асинхронно ждут своей очереди, не занимая потоки и соединения с БД.
	/// Защищает сервис от клиента, который шлёт пачки параллельных запросов
	/// </summary>
	internal class PerUserConcurrencyLimitMiddleware
	{
		/// <summary>
		/// Секция настроек в appsettings
		/// </summary>
		private const string _configurationSection = "RequestLimits:PerUser";

		/// <summary>
		/// Значение заголовка ответа Retry-After при ответе 429: через сколько секунд клиенту стоит повторить запрос.
		/// </summary>
		private const string _retryAfterSeconds = "5";

		private readonly RequestDelegate _next;

		private readonly ILogger<PerUserConcurrencyLimitMiddleware> _logger;

		/// <summary>
		/// Сколько запросов одного пользователя выполняется одновременно. 0 и меньше — ограничение выключено
		/// </summary>
		private readonly int _maxConcurrentRequests;

		/// <summary>
		/// Сколько запрос ждёт свободного места, прежде чем получить 429
		/// </summary>
		private readonly TimeSpan _queueTimeout;

		/// <summary>
		/// Блокировка для доступа к <see cref="_semaphores"/>
		/// </summary>
		private readonly object _semaphoresLock = new object();

		/// <summary>
		/// Семафоры пользователей, у которых сейчас есть запросы в обработке или в очереди. Ключ — логин пользователя
		/// </summary>
		private readonly Dictionary<string, UserSemaphore> _semaphores = new Dictionary<string, UserSemaphore>();

		public PerUserConcurrencyLimitMiddleware(
			RequestDelegate next,
			ILogger<PerUserConcurrencyLimitMiddleware> logger,
			IConfiguration configuration)
		{
			_next = next ?? throw new ArgumentNullException(nameof(next));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			if(configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			var section = configuration.GetSection(_configurationSection);

			_maxConcurrentRequests = section.GetValue("MaxConcurrentRequests", 5);
			_queueTimeout = TimeSpan.FromSeconds(section.GetValue("QueueTimeoutSeconds", 30));

			_logger.LogInformation(
				"Ограничение одновременных запросов пользователя: одновременно {MaxConcurrentRequests}, ожидание в очереди до {QueueTimeoutSeconds} с",
				_maxConcurrentRequests,
				_queueTimeout.TotalSeconds);
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var userName = context.User?.Identity?.IsAuthenticated == true
				? context.User.Identity.Name
				: null;

			// 0 и меньше — ограничение выключено
			if(_maxConcurrentRequests <= 0 || string.IsNullOrWhiteSpace(userName))
			{
				await _next(context);
				return;
			}

			var userSemaphore = AcquireUserSemaphore(userName);

			try
			{
				if(!await WaitForSlotAsync(context, userSemaphore.Semaphore, userName))
				{
					return;
				}

				try
				{
					await _next(context);
				}
				finally
				{
					userSemaphore.Semaphore.Release();
				}
			}
			finally
			{
				ReleaseUserSemaphore(userName, userSemaphore);
			}
		}

		/// <summary>
		/// Ждёт свободного места для запроса пользователя.
		/// Возвращает false, если запрос не дождался очереди (ответ 429) или клиент его отменил
		/// </summary>
		private async Task<bool> WaitForSlotAsync(HttpContext context, SemaphoreSlim semaphore, string userName)
		{
			if(semaphore.Wait(0))
			{
				return true;
			}

			var stopwatch = Stopwatch.StartNew();

			try
			{
				if(!await semaphore.WaitAsync(_queueTimeout, context.RequestAborted))
				{
					_logger.LogWarning(
						"Запрос {RequestPath} пользователя {Username} не дождался очереди за {QueueTimeoutSeconds} с и отклонён",
						context.Request.Path,
						userName,
						_queueTimeout.TotalSeconds);

					context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
					context.Response.Headers["Retry-After"] = _retryAfterSeconds;

					return false;
				}
			}
			catch(OperationCanceledException) when(context.RequestAborted.IsCancellationRequested)
			{
				_logger.LogInformation(
					"Клиент пользователя {Username} отменил запрос {RequestPath}, пока тот ждал в очереди {QueueWaitMilliseconds} мс",
					userName,
					context.Request.Path,
					stopwatch.ElapsedMilliseconds);

				return false;
			}

			_logger.LogWarning(
				"Запрос {RequestPath} пользователя {Username} ждал в очереди {QueueWaitMilliseconds} мс из-за лимита одновременных запросов {MaxConcurrentRequests}",
				context.Request.Path,
				userName,
				stopwatch.ElapsedMilliseconds,
				_maxConcurrentRequests);

			return true;
		}

		private UserSemaphore AcquireUserSemaphore(string userName)
		{
			lock(_semaphoresLock)
			{
				if(!_semaphores.TryGetValue(userName, out var userSemaphore))
				{
					userSemaphore = new UserSemaphore(_maxConcurrentRequests);
					_semaphores[userName] = userSemaphore;
				}

				userSemaphore.References++;

				return userSemaphore;
			}
		}

		private void ReleaseUserSemaphore(string userName, UserSemaphore userSemaphore)
		{
			lock(_semaphoresLock)
			{
				userSemaphore.References--;

				if(userSemaphore.References == 0)
				{
					_semaphores.Remove(userName);
					userSemaphore.Semaphore.Dispose();
				}
			}
		}

		/// <summary>
		/// Семафор пользователя и количество его запросов, которые сейчас проходят через middleware.
		/// Когда запросов не остаётся, семафор удаляется, чтобы не хранить данные всех пользователей
		/// </summary>
		private sealed class UserSemaphore
		{
			public UserSemaphore(int maxConcurrentRequests)
			{
				Semaphore = new SemaphoreSlim(maxConcurrentRequests, maxConcurrentRequests);
			}

			public SemaphoreSlim Semaphore { get; }

			/// <summary>
			/// Изменяется только под блокировкой
			/// </summary>
			public int References { get; set; }
		}
	}
}
