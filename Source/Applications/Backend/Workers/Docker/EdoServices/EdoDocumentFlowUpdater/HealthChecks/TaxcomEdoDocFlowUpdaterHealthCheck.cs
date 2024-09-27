using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;

namespace EdoDocumentFlowUpdater.HealthChecks
{
	public class TaxcomEdoDocFlowUpdaterHealthCheck : VodovozHealthCheckBase
	{
		public TaxcomEdoDocFlowUpdaterHealthCheck(ILogger<TaxcomEdoDocFlowUpdaterHealthCheck> logger, IUnitOfWorkFactory unitOfWorkFactory)
			: base(logger, unitOfWorkFactory)
		{
		}

		protected override Task<VodovozHealthResultDto> GetHealthResult() => Task.FromResult(HealthResult);

		public VodovozHealthResultDto HealthResult { get; set; }
	}
}
