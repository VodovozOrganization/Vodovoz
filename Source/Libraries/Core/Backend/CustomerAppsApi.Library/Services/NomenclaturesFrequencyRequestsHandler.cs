using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CustomerAppsApi.Library.Services
{
	public class NomenclaturesFrequencyRequestsHandler : FrequencyRequestsHandler
	{
		public NomenclaturesFrequencyRequestsHandler(
			ILogger<FrequencyRequestsHandler> logger,
			IConfiguration configuration) : base(logger, configuration)
		{
			RequestLimitType = RequestLimitType.NomenclaturesRequestFrequencyLimit;
		}

		protected override RequestLimitType RequestLimitType { get; }
	}
}
