using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;

namespace EdoContactsUpdater.HealthChecks
{
	public class TaxcomEdoContactsUpdaterHealthCheck : VodovozHealthCheckBase
	{
		public TaxcomEdoContactsUpdaterHealthCheck(
			ILogger<TaxcomEdoContactsUpdaterHealthCheck> logger,
			IUnitOfWorkFactory unitOfWorkFactory)
			: base(logger, unitOfWorkFactory)
		{
		}

		protected override Task<VodovozHealthResultDto> GetHealthResult() => Task.FromResult(HealthResult);

		public VodovozHealthResultDto HealthResult { get; set; }
	}
}
