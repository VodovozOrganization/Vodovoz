using Mango.Vpbx.Client.Services;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Errors.Logistics;

namespace DriverAPI.Library.V6.Services
{
	/// <inheritdoc/>
	public class CallsService : ICallsService
	{
		private readonly ILogger<CallsService> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IMangoCallsService _mangoCallsService;
		private readonly IRouteListRepository _routeListRepository;

		/// <inheritdoc/>
		public CallsService(
			ILogger<CallsService> logger,
			IUnitOfWork uow,
			IMangoCallsService mangoCallsService,
			IRouteListRepository routeListRepository
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_mangoCallsService = mangoCallsService ?? throw new ArgumentNullException(nameof(mangoCallsService));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
		}

		/// <inheritdoc/>
		public async Task<Result<Guid>> MakeWebhookCall(int routeListId, Employee driver, string toNumber, CancellationToken cancellationToken)
		{
			if(driver is null)
			{
				throw new ArgumentNullException(nameof(driver));
			}

			if(string.IsNullOrWhiteSpace(toNumber))
			{
				throw new ArgumentException($"'{nameof(toNumber)}' cannot be null or whitespace.", nameof(toNumber));
			}

			var routeList =
				await _routeListRepository.GetRouteListByIdAsync(_uow, routeListId);

			if(routeList is null)
			{
				_logger.LogError(
					"Маршрутный лист с номером {RouteListId} не найден",
					routeListId);

				return Result.Failure<Guid>(RouteListErrors.CreateNotFound(routeListId));
			}

			if(routeList.Driver is null
				|| routeList.Driver.Id != driver.Id)
			{
				_logger.LogError(
					"Водитель с id {DriverId} пытается получить доступ к маршрутному листу с номером {RouteListId}, водителем которого является {RouteListDriverId}",
					driver.Id,
					routeListId,
					routeList.Driver?.Id);

				return Result.Failure<Guid>(Errors.Security.Authorization.RouteListAccessDenied);
			}

			var extension = "1234";
			var lineNumber = "79000000001";
			return await _mangoCallsService.MakeWebhookCall(extension, toNumber, lineNumber, cancellationToken);
		}
	}
}
