using System;
using System.Collections.Concurrent;
using CustomerAppsApi.Library.Dto;
using Gamma.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Services
{
	public abstract class FrequencyRequestsHandler
	{
		private DateTime _lastRequestTime;
		protected readonly ILogger<FrequencyRequestsHandler> Logger;
		protected readonly IConfigurationSection FrequencyMinutesLimitSection;
		protected readonly ConcurrentDictionary<Source, DateTime> RequestTimes;

		protected FrequencyRequestsHandler(
			ILogger<FrequencyRequestsHandler> logger,
			IConfiguration configuration)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			RequestTimes = new ConcurrentDictionary<Source, DateTime>();
			FrequencyMinutesLimitSection = 
				(configuration ?? throw new ArgumentNullException(nameof(configuration)))
				.GetSection("RequestsMinutesLimits");
		}
		
		protected abstract RequestLimitType RequestLimitType { get; }

		public bool CanRequest(Source source, string sourceName)
		{
			var now = DateTime.Now;
			_lastRequestTime = RequestTimes.GetOrAdd(source, now);
			var passedMinutes = _lastRequestTime == now ? 0d : (now - _lastRequestTime).TotalMinutes;
			var requestFrequencyMinutesLimit =
				FrequencyMinutesLimitSection.GetValue<int>(RequestLimitType.ToString());

			if(passedMinutes > 0 && passedMinutes < requestFrequencyMinutesLimit)
			{
				Logger.LogInformation(
					"Превышен {FrequencyLimit} для источника {Source}",
					RequestLimitType.GetEnumTitle(),
					sourceName);

				return false;
			}

			return true;
		}

		public bool TryUpdate(Source source)
		{
			return RequestTimes.TryUpdate(source, DateTime.Now, _lastRequestTime);
		}
	}
}
