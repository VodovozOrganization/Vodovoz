using System.Threading.Tasks;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Utils;

namespace MailjetEventsDistributorAPI.HealthChecks
{
	public class MailjetEventsDistributeHealthCheck : VodovozHealthCheckBase
	{
		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			var isHealthy = UrlExistsChecker.UrlExists("https://localhost:5001/Test");

			return new VodovozHealthResultDto
			{
				IsHealthy = isHealthy
			};
		}
	}
}
