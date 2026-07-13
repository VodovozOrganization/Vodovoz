using System;
using CustomerAppsApi.Library.V1.Dto;
using CustomerAppsApi.Library.V1.Models;
using CustomerAppsApi.Library.V1.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Core.Domain.Clients;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.V1.Controllers
{
	[ApiController]
	[Route("/api/")]
	public class RentPackagesController : ControllerBase
	{
		private readonly ILogger<RentPackagesController> _logger;
		private readonly RentPackagesFrequencyRequestsHandler _rentPackagesFrequencyRequestsHandler;
		private readonly IRentPackageModel _rentPackageModel;

		public RentPackagesController(
			ILogger<RentPackagesController> logger,
			RentPackagesFrequencyRequestsHandler rentPackagesFrequencyRequestsHandler,
			IRentPackageModel rentPackageModel)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_rentPackagesFrequencyRequestsHandler =
				rentPackagesFrequencyRequestsHandler ?? throw new ArgumentNullException(nameof(rentPackagesFrequencyRequestsHandler));
			_rentPackageModel = rentPackageModel ?? throw new ArgumentNullException(nameof(rentPackageModel));
		}

		[HttpGet("GetFreeRentPackages")]
		public FreeRentPackagesDto GetFreeRentPackages([FromQuery] Source source)
		{
			var sourceName = source.GetEnumTitle();
			try
			{
				var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);

				var canRequest = isDryRun || _rentPackagesFrequencyRequestsHandler.CanRequest(source, sourceName);

				if(!canRequest)
				{
					return new FreeRentPackagesDto
					{
						ErrorMessage = "Превышен интервал обращений"
					};
				}

				var rentPackages = _rentPackageModel.GetFreeRentPackages(source);

				if(!isDryRun)
				{
					_rentPackagesFrequencyRequestsHandler.TryUpdate(source);
				}

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
