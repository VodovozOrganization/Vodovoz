using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CustomerAppsApi.Library.Services
{
	public class PricesFrequencyRequestsHandler : FrequencyRequestsHandler
	{
		public PricesFrequencyRequestsHandler(
			ILogger<FrequencyRequestsHandler> logger,
			IConfiguration configuration) : base(logger, configuration)
		{
			RequestLimitType = RequestLimitType.PricesAndStocksRequestFrequencyLimit;
		}

		protected override RequestLimitType RequestLimitType { get; }
	}
}
