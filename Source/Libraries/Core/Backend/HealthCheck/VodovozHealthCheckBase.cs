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
			_logger.LogInformation("Поступил запрос на информацию о здоровье.");

			bool isDbConnected;
			VodovozHealthResultDto healthResult;

			try
			{
				if(_unitOfWorkFactory != null)
				{
					isDbConnected = CheckDbConnection();

					if(!isDbConnected)
					{
						return HealthCheckResult.Unhealthy("Проблема с БД.");
					}
				}

				healthResult = await GetHealthResult();
			}
			catch(Exception e)
			{
				return HealthCheckResult.Unhealthy("Возникло искючение во время проверки здоровья.", e);
			}

			if(healthResult == null)
			{
				return HealthCheckResult.Unhealthy("Пустой результат проверки.");
			}

			if(healthResult.IsHealthy)
			{
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

			_logger.LogInformation(failedMessage);

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
