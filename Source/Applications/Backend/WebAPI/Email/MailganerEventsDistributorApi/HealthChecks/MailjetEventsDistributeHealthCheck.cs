using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

namespace MailganerEventsDistributorApi.HealthChecks
{
	public class MailjetEventsDistributeHealthCheck : VodovozHealthCheckBase
	{
		private readonly IConfiguration _configuration;

		public MailjetEventsDistributeHealthCheck(ILogger<MailjetEventsDistributeHealthCheck> logger, IConfiguration configuration, IUnitOfWorkFactory unitOfWorkFactory)
		: base(logger, unitOfWorkFactory)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthSection = _configuration.GetSection("Health");
			var baseAddress = healthSection.GetValue<string>("BaseAddress");

			var isHealthy = ResponseHelper.CheckUriExists($"{baseAddress}/Test");

			return new VodovozHealthResultDto
			{
				IsHealthy = isHealthy
			};
		}
	}
}
