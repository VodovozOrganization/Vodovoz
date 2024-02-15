using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using VodovozHealthCheck.Dto;

namespace VodovozHealthCheck
{
	public abstract class VodovozHealthCheckBase : IHealthCheck
	{
		private readonly ILogger<VodovozHealthCheckBase> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public VodovozHealthCheckBase(ILogger<VodovozHealthCheckBase> logger, IUnitOfWorkFactory unitOfWorkFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new ())
		{
			_logger.LogInformation("Поступил запрос на информацию о здоровье.");

			VodovozHealthResultDto healthResult;

			try
			{
				CheckDbConnection();
				healthResult = await GetHealthResult();
			}
			catch(Exception e)
			{
				return HealthCheckResult.Unhealthy("Возникло искючение во время проверки здоровья.", e);
			}

			if(healthResult == null )
			{
				return HealthCheckResult.Unhealthy("Пустой результат проверки.");
			}

			if(healthResult.IsHealthy )
			{
				return HealthCheckResult.Healthy();
			}

			var unhealthyDictionary = new Dictionary<string, object>
			{
				{ "results", healthResult.AdditionalUnhealthyResults }
			};

			var failedMessage = "Проверка не пройдена";

			_logger.LogInformation(failedMessage);

			return HealthCheckResult.Unhealthy(failedMessage, null, unhealthyDictionary);
		}

		private void CheckDbConnection()
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("HealthCheck"))
			{
				var query = uow.Session.CreateSQLQuery("SELECT 1");
				query.UniqueResult();
			}
		}

		protected abstract Task<VodovozHealthResultDto> GetHealthResult();
	}
}
