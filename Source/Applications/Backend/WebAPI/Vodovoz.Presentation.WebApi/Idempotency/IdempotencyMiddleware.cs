using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Vodovoz.Presentation.WebApi.Caching.Idempotency;

namespace Vodovoz.Presentation.WebApi.Idempotency
{
	/// <summary>
	/// Идемпотентность запросов к методам, отмеченным <see cref="IdempotentAttribute"/>.
	/// Первый запрос с ключом из заголовка <see cref="IdempotencyRequestHeadersNames.IdempotencyKey"/> выполняется,
	/// успешный (2xx) ответ сохраняется в хранилище. Повтор с тем же ключом получает сохранённый ответ без выполнения метода,
	/// а пока первый запрос выполняется ждёт его результат
	/// При недоступном хранилище или без ключа запросы выполняются как обычно
	/// </summary>
	internal sealed class IdempotencyMiddleware
	{
		private static readonly TimeSpan _replayPollInterval = TimeSpan.FromMilliseconds(500);

		private readonly ILogger<IdempotencyMiddleware> _logger;
		private readonly RequestDelegate _next;
		private readonly IFeatureManager _featureManager;
		private readonly IIdempotencyStore _idempotencyStore;
		private readonly IdempotencyTimingsProvider _idempotencyTimingsProvider;
		private readonly IdempotencyKeyBuilder _idempotencyKeyBuilder;

		public IdempotencyMiddleware(
			ILogger<IdempotencyMiddleware> logger,
			RequestDelegate next,
			IFeatureManager featureManager,
			IIdempotencyStore idempotencyStore,
			IdempotencyTimingsProvider idempotencyTimingsProvider,
			IdempotencyKeyBuilder idempotencyKeyBuilder)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_next = next ?? throw new ArgumentNullException(nameof(next));
			_featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
			_idempotencyStore = idempotencyStore ?? throw new ArgumentNullException(nameof(idempotencyStore));
			_idempotencyTimingsProvider = idempotencyTimingsProvider ?? throw new ArgumentNullException(nameof(idempotencyTimingsProvider));
			_idempotencyKeyBuilder = idempotencyKeyBuilder ?? throw new ArgumentNullException(nameof(idempotencyKeyBuilder));
		}

		public async Task InvokeAsync(HttpContext context)
		{
			if(context.GetEndpoint()?.Metadata.GetMetadata<IdempotentAttribute>() is null
				|| !await _featureManager.IsEnabledAsync(FeatureFlags.Idempotency))
			{
				await _next(context);
				return;
			}

			var identity = context.User?.Identity;

			if(identity?.IsAuthenticated != true)
			{
				await _next(context);
				return;
			}

			if(string.IsNullOrEmpty(identity.Name))
			{
				_logger.LogWarning(
					"Идемпотентность не применена к запросу {RequestPath}: у аутентифицированного пользователя нет имени",
					context.Request.Path);

				await _next(context);
				return;
			}

			if(!context.Request.Headers.TryGetValue(IdempotencyRequestHeadersNames.IdempotencyKey, out var rawIdempotencyKey)
				|| rawIdempotencyKey.Count == 0)
			{
				await _next(context);
				return;
			}

			if(!Guid.TryParse(rawIdempotencyKey.ToString(), out var idempotencyKey))
			{
				_logger.LogWarning(
					"Идемпотентность не применена к запросу {RequestPath}: некорректный заголовок {HeaderName}: {RawValue}",
					context.Request.Path,
					IdempotencyRequestHeadersNames.IdempotencyKey,
					rawIdempotencyKey.ToString());

				await _next(context);
				return;
			}

			if(!_idempotencyTimingsProvider.TryGetTimings(out var timings))
			{
				await _next(context);
				return;
			}

			var key = _idempotencyKeyBuilder.Build(context, idempotencyKey);

			switch(await _idempotencyStore.TryBeginAsync(key, CreateMarker(), timings.MarkerLifetime))
			{
				case IdempotencyBeginResult.Started:
					await ExecuteAndSaveAsync(context, key, timings);
					return;
				case IdempotencyBeginResult.AlreadyExists:
					if(await WaitForFirstRequestAsync(context, key, timings))
					{
						await ExecuteAndSaveAsync(context, key, timings);
					}
					return;
				default:
					// Хранилище недоступно, о другом исполнителе ничего не известно, выполняем как без идемпотентности
					await _next(context);
					return;
			}
		}

		/// <summary>
		/// Ожидание результата первого запроса с тем же ключом.
		/// Другой исполнитель уже есть, поэтому при ошибках хранилища запрос сам не выполняется:
		/// только ответ первого, 409 по таймауту или выполнение, если метка исчезла и её удалось поставить заново
		/// </summary>
		/// <returns><see langword="true"/>, если метка поставлена текущим запросом и его нужно выполнить</returns>
		private async Task<bool> WaitForFirstRequestAsync(HttpContext context, string key, IdempotencyTimings timings)
		{
			var stopwatch = Stopwatch.StartNew();
			var isUnavailableLogged = false;
			var waitReason = string.Empty;

			try
			{
				while(true)
				{
					var readResult = await _idempotencyStore.GetAsync(key);

					switch(readResult.Status)
					{
						case IdempotencyReadStatus.Found when readResult.Record.State == IdempotencyRecordState.Completed:
							await WriteSavedResponseAsync(context, key, readResult.Record);
							return false;
						case IdempotencyReadStatus.Found:
							waitReason = "запрос ещё выполняется";
							break;
						case IdempotencyReadStatus.NotFound:
							// Первый запрос не удался и снял метку, либо метка истекла
							var beginResult = await _idempotencyStore.TryBeginAsync(key, CreateMarker(), timings.MarkerLifetime);

							if(beginResult == IdempotencyBeginResult.Started)
							{
								return true;
							}

							waitReason = beginResult == IdempotencyBeginResult.AlreadyExists
								? "запрос ещё выполняется"
								: "хранилище недоступно";
							break;
						default:
							if(!isUnavailableLogged)
							{
								_logger.LogWarning(
									"Не удалось прочитать результат первого запроса по ключу {IdempotencyStorageKey}, ожидание продолжается",
									key);

								isUnavailableLogged = true;
							}

							waitReason = "хранилище недоступно";
							break;
					}

					var remaining = timings.ReplayWaitTimeout - stopwatch.Elapsed;

					if(remaining <= TimeSpan.Zero)
					{
						await WriteStillProcessingAsync(context, key, waitReason);
						return false;
					}

					await Task.Delay(remaining < _replayPollInterval ? remaining : _replayPollInterval, context.RequestAborted);
				}
			}
			catch(OperationCanceledException) when(context.RequestAborted.IsCancellationRequested)
			{
				_logger.LogInformation(
					"Клиент отключился во время ожидания результата первого запроса по ключу {IdempotencyStorageKey}",
					key);

				return false;
			}
		}

		private async Task ExecuteAndSaveAsync(HttpContext context, string key, IdempotencyTimings timings)
		{
			var originalBody = context.Response.Body;
			using var buffer = new MemoryStream();
			context.Response.Body = buffer;

			var stopwatch = Stopwatch.StartNew();

			try
			{
				await _next(context);
			}
			catch
			{
				await _idempotencyStore.RemoveAsync(key);
				throw;
			}
			finally
			{
				context.Response.Body = originalBody;
			}

			stopwatch.Stop();

			if(stopwatch.Elapsed > timings.MarkerLifetime)
			{
				_logger.LogWarning(
					"Запрос {RequestPath} выполнялся {Elapsed}, дольше срока метки обработки {MarkerLifetime}. "
						+ "Увеличьте настройку Idempotency.<имя API>.MarkerLifetime",
					context.Request.Path,
					stopwatch.Elapsed,
					timings.MarkerLifetime);
			}

			if(IsCacheable(context.Response.StatusCode))
			{
				var completedRecord = IdempotencyRecord.CreateCompleted(
					context.Response.StatusCode,
					context.Response.ContentType,
					buffer.ToArray(),
					GetActionTimeUtc(context),
					DateTime.UtcNow);

				if(!await _idempotencyStore.SaveCompletedAsync(key, completedRecord, timings.ResponseLifetime))
				{
					_logger.LogWarning(
						"Ответ запроса {RequestPath} по ключу {IdempotencyStorageKey} не сохранён, метка обработки оставлена на {MarkerLifetime}: "
							+ "повторы с этим ключом получат 409",
						context.Request.Path,
						key,
						timings.MarkerLifetime);
				}
			}
			else
			{
				await _idempotencyStore.RemoveAsync(key);
			}

			buffer.Position = 0;
			await buffer.CopyToAsync(originalBody);
		}

		/// <summary>
		/// Можно ли сохранить ответ для повторов. Сохраняются только 2xx
		/// В решении 2xx не всегда означает успех (например, <c>Problem(..., 202)</c> или 200 с текстом ошибки в теле),
		/// такие ответы тоже сохраняются
		/// </summary>
		private static bool IsCacheable(int statusCode) =>
			statusCode >= StatusCodes.Status200OK && statusCode < StatusCodes.Status300MultipleChoices;

		private async Task WriteSavedResponseAsync(HttpContext context, string key, IdempotencyRecord record)
		{
			context.Response.StatusCode = record.StatusCode;

			if(!string.IsNullOrEmpty(record.ContentType))
			{
				context.Response.ContentType = record.ContentType;
			}

			context.Response.Headers[IdempotencyResponseHeadersNames.IdempotentReplayed] = "true";

			if(record.Body?.Length > 0)
			{
				await context.Response.Body.WriteAsync(record.Body, 0, record.Body.Length);
			}

			_logger.LogInformation(
				"Отдан сохранённый ответ {StatusCode} по ключу {IdempotencyStorageKey}, метод не выполнялся",
				record.StatusCode,
				key);
		}

		private async Task WriteStillProcessingAsync(HttpContext context, string key, string waitReason)
		{
			_logger.LogWarning(
				"Не дождались результата первого запроса по ключу {IdempotencyStorageKey} ({WaitReason}), возвращён 409",
				key,
				waitReason);

			var problemDetails = new ProblemDetails
			{
				Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
				Title = "Запрос с этим ключом ещё обрабатывается",
				Status = StatusCodes.Status409Conflict,
				Instance = context.Request.Path
			};

			context.Response.StatusCode = StatusCodes.Status409Conflict;
			await context.Response.WriteAsJsonAsync(problemDetails, null, "application/problem+json");
		}

		private static IdempotencyRecord CreateMarker() =>
			IdempotencyRecord.CreateProcessing(DateTime.UtcNow, Environment.MachineName);

		private static DateTime? GetActionTimeUtc(HttpContext context)
		{
			var actionTimeUtcProvider = context.RequestServices.GetService<IActionTimeUtcProvider>();

			if(actionTimeUtcProvider != null && actionTimeUtcProvider.TryGetActionTimeUtc(out var actionTimeUtc, out _))
			{
				return actionTimeUtc;
			}

			return null;
		}
	}
}
