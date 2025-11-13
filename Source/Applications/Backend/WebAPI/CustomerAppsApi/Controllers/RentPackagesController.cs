using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Services;
using CustomerAppsApi.Models;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Controllers
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
				if(!_rentPackagesFrequencyRequestsHandler.CanRequest(source, sourceName))
				{
					return new FreeRentPackagesDto
					{
						ErrorMessage = "Превышен интервал обращений"
					};
				}

				var rentPackages = _rentPackageModel.GetFreeRentPackages(source);
				_rentPackagesFrequencyRequestsHandler.TryUpdate(source);
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
