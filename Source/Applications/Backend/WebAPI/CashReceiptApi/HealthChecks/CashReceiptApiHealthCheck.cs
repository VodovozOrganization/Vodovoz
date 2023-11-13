using System.Threading.Tasks;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;

namespace CashReceiptApi.HealthChecks
{
	public class CashReceiptApiHealthCheck : VodovozHealthCheckBase
	{
		protected override Task<VodovozHealthResultDto> GetHealthResult() => Task.FromResult<VodovozHealthResultDto>(new() { IsHealthy = IsHealthy });

		public bool IsHealthy { get; set; }
	}
}
