using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CustomerAppsApi.Library.Services
{
	public class SelfDeliveriesAddressesFrequencyRequestsHandler : FrequencyRequestsHandler
	{
		public SelfDeliveriesAddressesFrequencyRequestsHandler(
			ILogger<FrequencyRequestsHandler> logger,
			IConfiguration configuration) : base(logger, configuration)
		{
			RequestLimitType = RequestLimitType.SelfDeliveriesAddressesRequestFrequencyLimit;
		}

		protected override RequestLimitType RequestLimitType { get; }
	}
}
