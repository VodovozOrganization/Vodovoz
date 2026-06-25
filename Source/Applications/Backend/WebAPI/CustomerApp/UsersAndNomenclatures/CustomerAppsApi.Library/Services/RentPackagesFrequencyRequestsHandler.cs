using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CustomerAppsApi.Library.Services
{
	public class RentPackagesFrequencyRequestsHandler : FrequencyRequestsHandler
	{
		public RentPackagesFrequencyRequestsHandler(
			ILogger<FrequencyRequestsHandler> logger,
			IConfiguration configuration) : base(logger, configuration)
		{
			RequestLimitType = RequestLimitType.RentPackagesRequestFrequencyLimit;
		}

		protected override RequestLimitType RequestLimitType { get; }
	}
}
