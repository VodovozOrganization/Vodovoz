using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading.Tasks;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

namespace PayPageAPI.HealthChecks
{
	public class PayPageHealthCheck : VodovozHealthCheckBase
	{
		private readonly IConfiguration _configuration;

		public PayPageHealthCheck(ILogger<PayPageHealthCheck> logger, IConfiguration configuration, IUnitOfWorkFactory unitOfWorkFactory)
			: base(logger, unitOfWorkFactory)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		protected override Task<VodovozHealthResultDto> GetHealthResult()
		{
			var healthSection = _configuration.GetSection("Health");
			var baseAddress = healthSection.GetValue<string>("BaseAddress");
			var guid = healthSection.GetValue<string>("Variables:Guid");

			var isHealthy = ResponseHelper.CheckUriExists($"{baseAddress}/{guid}");

			return Task.FromResult(new VodovozHealthResultDto
			{
				IsHealthy = isHealthy
			});
		}
	}
}
