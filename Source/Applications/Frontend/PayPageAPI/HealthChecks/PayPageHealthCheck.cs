using System.Threading.Tasks;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Utils;

namespace PayPageAPI.HealthChecks
{
	public class PayPageHealthCheck : VodovozHealthCheckBase
	{
		protected override Task<VodovozHealthResultDto> GetHealthResult()
		{
			var isHealthy = UrlExistsChecker.UrlExists("https://sbp.vodovoz-spb.ru:4001/f9758536-733e-479d-9190-888d76572400");

			return Task.FromResult(new VodovozHealthResultDto
			{
				IsHealthy = isHealthy
			});
		}
	}
}
