using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using VodovozHealthCheck.Dto;

namespace VodovozHealthCheck
{
	public abstract class VodovozHealthCheckBase : IHealthCheck
	{
		private readonly ILogger<VodovozHealthCheckBase> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public VodovozHealthCheckBase(ILogger<VodovozHealthCheckBase> logger, IUnitOfWorkFactory unitOfWorkFactory = null)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
		{
			_logger.LogInformation("Поступил запрос на информацию о здоровье в базовый класс.");

			var checkMessage = "Проверяем здоровье";

			bool isDbConnected;
			VodovozHealthResultDto healthResult;

			try
			{
				if(_unitOfWorkFactory != null)
				{
					_logger.LogInformation("{CheckMessage}: Соединение с БД.", checkMessage);

					isDbConnected = CheckDbConnection();

					_logger.LogInformation("{CheckMessage}: Проверили соединение с БД, результат: {IsDbConnected}", checkMessage, isDbConnected);

					if(!isDbConnected)
					{
						return HealthCheckResult.Unhealthy("Проблема с БД.");
					}
				}

				_logger.LogInformation("{CheckMessage}: Вызываем проверку из сервиса.", checkMessage);

				healthResult = await GetHealthResult();

				_logger.LogInformation("{CheckMessage}: Проверка из сервиса завершена, результат: IsHealthy {IsHealthy}", checkMessage, healthResult.IsHealthy);
			}
			catch(Exception e)
			{
				_logger.LogError("{CheckMessage}: Не удалось выполнить проверку, ошибка: {HealthCheckException}", checkMessage, e);
				
				return HealthCheckResult.Unhealthy("Возникло искючение во время проверки здоровья.", e);
			}

			if(healthResult == null)
			{
				_logger.LogInformation("{CheckMessage}: Пустой результат проверки.", checkMessage);
				
				return HealthCheckResult.Unhealthy("Пустой результат проверки.");
			}

			if(healthResult.IsHealthy)
			{
				_logger.LogInformation("{CheckMessage}: Возвращаем итоговый результат IsHealthy: {IsHealthy}", checkMessage, healthResult.IsHealthy);
				
				return HealthCheckResult.Healthy("Проверка пройдена успешно");
			}
			
			Dictionary<string, object> unhealthyDictionary = null;

			if(Enumerable.Any(healthResult.AdditionalUnhealthyResults))
			{
				unhealthyDictionary = new Dictionary<string, object>
				{
					{
						"results",
						healthResult.AdditionalUnhealthyResults
					}
				};
			}
			
			var failedMessage = "Проверка не пройдена";

			_logger.LogWarning("{CheckMessage}: {FailedMessage}", checkMessage, failedMessage);
			
			return HealthCheckResult.Unhealthy(failedMessage, null, unhealthyDictionary);
		}

		private bool CheckDbConnection()
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("HealthCheck"))
			{
				var query = uow.Session.CreateSQLQuery("SELECT 1");
				try
				{
					var result = query.UniqueResult();
					return result != null;
				}
				catch(Exception)
				{
					return false;
				}
			}
		}

		protected abstract Task<VodovozHealthResultDto> GetHealthResult();
	}
}
