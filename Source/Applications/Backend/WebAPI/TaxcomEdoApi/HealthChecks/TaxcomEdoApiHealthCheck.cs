using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;

namespace TaxcomEdoApi.HealthChecks
{
	public class TaxcomEdoApiHealthCheck : VodovozHealthCheckBase
	{
		public TaxcomEdoApiHealthCheck(ILogger<TaxcomEdoApiHealthCheck> logger, IUnitOfWorkFactory unitOfWorkFactory)
			: base(logger, unitOfWorkFactory)
		{
		}

		protected override Task<VodovozHealthResultDto> GetHealthResult() => Task.FromResult<VodovozHealthResultDto>(new() { IsHealthy = IsHealthy });

		public bool IsHealthy { get; set; }
	}
}
