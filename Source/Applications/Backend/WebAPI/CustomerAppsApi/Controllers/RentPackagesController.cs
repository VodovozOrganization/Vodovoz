using System;
using System.Collections.Concurrent;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Models;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/")]
	public class RentPackagesController : ControllerBase
	{
		private readonly ILogger<CounterpartyController> _logger;
		private readonly IRentPackageModel _rentPackageModel;
		private readonly IConfigurationSection _requestsLimitsSection;
		private static readonly ConcurrentDictionary<Source, DateTime> _requestTimes = new ConcurrentDictionary<Source, DateTime>();

		public RentPackagesController(
			ILogger<CounterpartyController> logger,
			IConfiguration configuration,
			IRentPackageModel rentPackageModel)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_rentPackageModel = rentPackageModel ?? throw new ArgumentNullException(nameof(rentPackageModel));
			_requestsLimitsSection = configuration.GetSection("RequestsMinutesLimits");
		}

		[HttpGet("GetFreeRentPackages")]
		public FreeRentPackagesDto GetFreeRentPackages([FromQuery] Source source)
		{
			var sourceName = source.GetEnumTitle();
			try
			{
				_logger.LogInformation("Поступил запрос на выборку бесплатных пакетов аренды от источника {Source}", sourceName);
				var now = DateTime.Now;
				var lastRequestTime = _requestTimes.GetOrAdd(source, now);
				var passedTimeMinutes = lastRequestTime == now ? 0d : (now - lastRequestTime).TotalMinutes;
				var requestFrequencyMinutesLimit = _requestsLimitsSection.GetValue<int>("FreePackagesRequestFrequencyLimit");

				if(passedTimeMinutes > 0 && passedTimeMinutes < requestFrequencyMinutesLimit)
				{
					_logger.LogInformation("Превышен интервал обращений для источника {Source}", sourceName);
					return new FreeRentPackagesDto
					{
						ErrorMessage = "Превышен интервал обращений"
					};
				}

				var rentPackages = _rentPackageModel.GetFreeRentPackages(source);
				_requestTimes.TryUpdate(source, DateTime.Now, lastRequestTime);
				return rentPackages;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении бесплатных пакетов аренды {Source}", sourceName);
				return new FreeRentPackagesDto
				{
					ErrorMessage = e.Message
				};
			}
		}
	}
}
