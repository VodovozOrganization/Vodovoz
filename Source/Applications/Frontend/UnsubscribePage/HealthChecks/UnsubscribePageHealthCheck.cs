using System.Threading.Tasks;
using VodovozHealthCheck;
using VodovozHealthCheck.Utils;

namespace UnsubscribePage.HealthChecks
{
	public class UnsubscribePageHealthCheck : VodovozHealthCheckBase
	{
		protected override  Task<VodovozHealthResultDto> GetHealthResult()
		{
			var isHealthy = UrlExistsChecker.UrlExists("http://maileventsapi.vod.qsolution.ru:7093/1049b7ef-825b-46b7-87c9-b234af7f6d5e");

			return Task.FromResult(new VodovozHealthResultDto
			{
				IsHealthy = isHealthy
			});
		}
	}
}
