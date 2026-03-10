using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using QS.DomainModel.UoW;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Extensions;
using VodovozHealthCheck.Providers;

namespace VodovozHealthCheck
{
	public abstract class VodovozHealthCheckBase : IHealthCheck
	{
		private readonly ILogger<VodovozHealthCheckBase> _logger;
		protected readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IBusControl _busControl;
		
		protected IHealthCheckServiceInfoProvider ServiceInfoProvider { get; }

		private static readonly AsyncRetryPolicy<VodovozHealthResultDto> _serviceRetryPolicy = Policy<VodovozHealthResultDto>
			.Handle<Exception>()
			.OrResult(r => !r.IsHealthy)
			.WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromMilliseconds(500 * retryAttempt));

		/// <summary>
		/// Конструктор HealthCheck
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="unitOfWorkFactory">Необходим для проверки соединения с БД</param>
		/// <param name="busControl">Необходим для проверки состояния брокера сообщений</param>
		/// <exception cref="ArgumentNullException"></exception>
		protected VodovozHealthCheckBase(
			ILogger<VodovozHealthCheckBase> logger,
			IHealthCheckServiceInfoProvider serviceInfoProvider,
			IUnitOfWorkFactory unitOfWorkFactory = null,
			IBusControl busControl = null)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			ServiceInfoProvider = serviceInfoProvider ?? throw new ArgumentNullException(nameof(serviceInfoProvider));
			_unitOfWorkFactory = unitOfWorkFactory;
			_busControl = busControl;
		}

		/// <summary>
		/// Реализация интерфейса IHealthCheck
		/// </summary>
		/// <param name="context"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Поступил запрос на информацию о работоспособности в базовый класс.");

			const string checkMessage = "Проверяем работоспособность";

			var _unhealthyMessage =
				$"Обнаружены проблемы в сервисе <{ServiceInfoProvider.Name}>. Могут быть недоступны: {ServiceInfoProvider.DetailedDescription}";

			var healthResult = new VodovozHealthResultDto { IsHealthy = false };

			try
			{
				if(_unitOfWorkFactory != null)
				{
					_logger.LogInformation("{CheckMessage}: Соединение с БД.", checkMessage);

					var dbHealth = await CheckDbConnectionAsync(cancellationToken);

					_logger.LogInformation("{CheckMessage}: Проверили соединение с БД, результат: {IsDbConnected}", checkMessage, dbHealth.IsHealthy);

					if(!dbHealth.IsHealthy)
					{
						return HealthCheckResult.Unhealthy("Проблема соединения с БД.", data: dbHealth.ToUnhealthyDataInfoResponse());
					}
				}

				if(_busControl != null)
				{
					var busHealth = CheckBusHealth();

					if(!busHealth.IsHealthy)
					{
						return HealthCheckResult.Unhealthy("Проблема с брокером сообщений.", data: busHealth.ToUnhealthyDataInfoResponse());
					}
				}

				_logger.LogInformation("{CheckMessage}: Вызываем проверку из сервиса.", checkMessage);

				// Пока ищем причины проблем с сетью, пробуем без повтора
				// healthResult = await _serviceRetryPolicy.ExecuteAsync(CheckServiceHealthAsync, cancellationToken);

				healthResult = await CheckServiceHealthAsync(cancellationToken);

				_logger.LogInformation("{CheckMessage}: Проверка из сервиса завершена, результат: IsHealthy {IsHealthy}", checkMessage,
					healthResult.IsHealthy);
			}
			catch(Exception e)
			{
				_logger.LogError("{CheckMessage}: Не удалось выполнить проверку, ошибка: {HealthCheckException}", checkMessage, e);

				return HealthCheckResult.Unhealthy("Возникло исключение во время проверки работоспособности.", e);
			}

			if(healthResult.IsHealthy)
			{
				_logger.LogInformation("{CheckMessage}: Возвращаем итоговый результат IsHealthy: {IsHealthy}", checkMessage, healthResult.IsHealthy);

				return HealthCheckResult.Healthy("Проверка пройдена успешно.");
			}

			const string failedMessage = "Проверка не пройдена!";

			_logger.LogWarning("{CheckMessage}: {FailedMessage}", checkMessage, failedMessage);

			return HealthCheckResult.Unhealthy(_unhealthyMessage, data: healthResult.ToUnhealthyDataInfoResponse());
		}

		/// <summary>
		/// Проверка соединения с БД
		/// </summary>
		/// <returns></returns>
		private async Task<VodovozHealthResultDto> CheckDbConnectionAsync(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("HealthCheck"))
			{
				var query = uow.Session.CreateSQLQuery("SELECT 1");
				try
				{
					var result = await query.UniqueResultAsync(cancellationToken);

					return new VodovozHealthResultDto { IsHealthy = result != null };
				}
				catch(Exception e)
				{
					return new VodovozHealthResultDto
					{
						IsHealthy = false,
						AdditionalUnhealthyResults = new HashSet<string> { $"Проблема с БД: {e.Message}" }
					};
				}
			}
		}

		/// <summary>
		/// Проверка состояния брокера сообщений
		/// </summary>
		/// <returns></returns>
		private VodovozHealthResultDto CheckBusHealth()
		{
			try
			{
				var health = _busControl.CheckHealth();

				if(health.Status == BusHealthStatus.Healthy)
				{
					return new VodovozHealthResultDto { IsHealthy = true };
				}

				var failureVodovozHealthResult = new VodovozHealthResultDto
				{
					IsHealthy = false,
					AdditionalUnhealthyResults = new HashSet<string> { "Проблема связи с брокером сообщений" }
				};

				failureVodovozHealthResult.AdditionalUnhealthyResults.UnionWith(
					health.Endpoints?.Select(e => $"{e.Key}: {e.Value}"));

				return failureVodovozHealthResult;
			}
			catch(Exception ex)
			{
				return new VodovozHealthResultDto
				{
					IsHealthy = false,
					AdditionalUnhealthyResults = new HashSet<string> { $"Проблема при проверке брокера сообщений: {ex.Message}" }
				};
			}
		}

		/// <summary>
		/// Выполняет специфичную для службы проверку работоспособности. Реализуется на стороне службы.
		/// </summary>
		/// <returns></returns>
		protected abstract Task<VodovozHealthResultDto> CheckServiceHealthAsync(CancellationToken cancellationToken);

		/// <summary>
		/// Безопасно выполняет метод проверки работоспособности, перехватывая любые исключения.
		/// Гарантирует, что ошибка в одной проверке не прервёт выполнение остальных проверок.
		/// </summary>
		/// <param name="checkMethodName">Название проверяемого метода для идентификации источника ошибки</param>
		/// <param name="checkFunc">Делегат метода проверки, который необходимо выполнить</param>
		/// <returns>
		/// Результат проверки работоспособности. При успехе возвращает результат выполнения checkFunc.
		/// При возникновении исключения возвращает VodovozHealthResultDto с IsHealthy = false 
		/// и сообщением об ошибке в AdditionalUnhealthyResults.
		/// </returns>
		protected async Task<VodovozHealthResultDto> ExecuteHealthCheckSafelyAsync(
			string checkMethodName,
			Func<string, Task<VodovozHealthResultDto>> checkFunc)
		{
			try
			{
				return await checkFunc(checkMethodName);
			}
			catch(Exception e)
			{
				return VodovozHealthResultDto.UnhealthyResult(
					$"Ошибка в методе <{checkMethodName}>: {e}");
			}
		}

		/// <summary>
		/// Объединяет результаты нескольких проверок работоспособности в один общий результат.
		/// Итоговая проверка считается успешной только если все входящие проверки успешны.
		/// Все сообщения об ошибках из неуспешных проверок объединяются в одну коллекцию.
		/// </summary>
		/// <param name="serviceName">Название сервиса</param>
		/// <param name="healthCheckTasks">Коллекция методов проверок работоспособности для объединения</param>
		/// <returns>
		/// Объединённый результат проверки работоспособности, где:
		/// - IsHealthy = true только если все проверки в healthResults успешны (IsHealthy = true)
		/// - AdditionalUnhealthyResults содержит все уникальные сообщения об ошибках из всех проверок
		/// </returns>
		protected async Task<VodovozHealthResultDto> ConcatHealthCheckResultsAsync(IEnumerable<Task<VodovozHealthResultDto>> healthCheckTasks)
		{
			var tasks = healthCheckTasks as Task<VodovozHealthResultDto>[]
			            ?? healthCheckTasks?.ToArray()
			            ?? Array.Empty<Task<VodovozHealthResultDto>>();

			if(tasks.Length == 0)
			{
				return new VodovozHealthResultDto
				{
					IsHealthy = true,
					AdditionalUnhealthyResults = new[] { $"Название сервиса: {ServiceInfoProvider.Name}" }.ToHashSet()
				};
			}

			try
			{
				var results = await Task.WhenAll(tasks);

				return new VodovozHealthResultDto
				{
					IsHealthy = results.All(r => r.IsHealthy),
					AdditionalUnhealthyResults = new[] { $"Название сервиса: {ServiceInfoProvider.Name}" }
						.Concat(results.SelectMany(r => r.AdditionalUnhealthyResults))
						.ToHashSet()
				};
			}
			catch
			{
				// Собираем результаты по каждой задаче, заменяя упавшие/отменённые на UnhealthyResult
				var collected = new List<VodovozHealthResultDto>(tasks.Length);

				foreach(var t in tasks)
				{
					if(t.IsCompletedSuccessfully)
					{
						collected.Add(t.Result);
						continue;
					}

					if(t.IsCanceled)
					{
						collected.Add(VodovozHealthResultDto.UnhealthyResult("Задача проверки отменена"));
						continue;
					}

					var ex = t.Exception?.GetBaseException();
					var message = ex != null ? ex.Message : "Неизвестная ошибка при выполнении проверки";
					collected.Add(VodovozHealthResultDto.UnhealthyResult($"Ошибка выполнения проверки: {message}"));
				}

				return new VodovozHealthResultDto
				{
					IsHealthy = collected.All(r => r.IsHealthy),
					AdditionalUnhealthyResults = new[] { $"Название сервиса: {ServiceInfoProvider.Name}" }
						.Concat(collected.SelectMany(r => r.AdditionalUnhealthyResults))
						.ToHashSet()
				};
			}
		}
	}
}
