using DriverApi.Contracts.V6;
using DriverApi.Contracts.V6.Requests;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.V6.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.Errors;

namespace DriverAPI.Controllers.V6
{
	/// <summary>
	/// Контроллер регистраций событий
	/// </summary>
	[ApiVersion("6.0")]
	[Authorize]
	public class RegistrationsController : VersionedController
	{
		private readonly ILogger<RegistrationsController> _logger;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IEmployeeService _employeeService;
		private readonly IDriverMobileAppActionRecordService _driverMobileAppActionRecordService;
		private readonly IRouteListService _routeListService;
		private readonly ITrackPointsService _trackPointsService;
		private readonly IActionTimeHelper _actionTimeHelper;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="userManager"></param>
		/// <param name="employeeService"></param>
		/// <param name="driverMobileAppActionRecordService"></param>
		/// <param name="routeListService"></param>
		/// <param name="trackPointsService"></param>
		/// <param name="actionTimeHelper"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public RegistrationsController(
			ILogger<RegistrationsController> logger,
			UserManager<IdentityUser> userManager,
			IEmployeeService employeeService,
			IDriverMobileAppActionRecordService driverMobileAppActionRecordService,
			IRouteListService routeListService,
			ITrackPointsService trackPointsService,
			IActionTimeHelper actionTimeHelper) : base(logger)
		{
			_logger = logger
				?? throw new ArgumentNullException(nameof(logger));
			_userManager = userManager
				?? throw new ArgumentNullException(nameof(userManager));
			_employeeService = employeeService
				?? throw new ArgumentNullException(nameof(employeeService));
			_driverMobileAppActionRecordService = driverMobileAppActionRecordService
				?? throw new ArgumentNullException(nameof(driverMobileAppActionRecordService));
			_routeListService = routeListService
				?? throw new ArgumentNullException(nameof(routeListService));
			_trackPointsService = trackPointsService
				?? throw new ArgumentNullException(nameof(trackPointsService));
			_actionTimeHelper = actionTimeHelper
				?? throw new ArgumentNullException(nameof(actionTimeHelper));
		}

		/// <summary>
		/// Регистрация действий произведенных в мобильном приложении водителей
		/// </summary>
		/// <param name="driverActionModels">Список действий из лога для регистрации</param>
		/// <returns></returns>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> RegisterDriverActionsAsync([FromBody] IEnumerable<DriverActionDto> driverActionModels)
		{
			await Task.CompletedTask;
			return NoContent();
		}

		/// <summary>
		/// Регистрация предположительных координат адреса
		/// </summary>
		/// <param name="routeListAddressCoordinate"></param>
		/// <returns></returns>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> RegisterRouteListAddressCoordinatesAsync(
			[FromBody] RouteListAddressCoordinateDto routeListAddressCoordinate)
		{
			_logger.LogInformation("Попытка регистрации предположительных координат для адреса {RouteListAddressId} пользователем {Username} User token: {AccessToken}",
				routeListAddressCoordinate.RouteListAddressId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var recievedTime = DateTime.Now;

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeService.GetByAPILogin(user.UserName);

			var resultMessage = "OK";

			var localActionTime = routeListAddressCoordinate.ActionTimeUtc.ToLocalTime();

			var timeCheckResult = _actionTimeHelper.CheckRequestTime(recievedTime, localActionTime);

			if(timeCheckResult.IsFailure)
			{
				return MapResult(timeCheckResult, errorStatusCode: StatusCodes.Status400BadRequest);
			}

			try
			{
				return MapResult(
					_routeListService.RegisterCoordinateForRouteListItem(
					routeListAddressCoordinate.RouteListAddressId,
					routeListAddressCoordinate.Latitude,
					routeListAddressCoordinate.Longitude,
					localActionTime,
					driver.Id),
					result =>
					{
						if(result.IsSuccess)
						{
							return StatusCodes.Status204NoContent;
						}

						var firstError = result.Errors.First();

						if(firstError == Vodovoz.Errors.Logistics.RouteListErrors.NotEnRouteState
							|| firstError == Vodovoz.Errors.Logistics.RouteListErrors.RouteListItem.NotEnRouteState)
						{
							return StatusCodes.Status400BadRequest;
						}

						if(firstError == Library.Errors.Security.Authorization.RouteListAccessDenied)
						{
							return StatusCodes.Status403Forbidden;
						}

						if(firstError == Vodovoz.Errors.Logistics.RouteListErrors.RouteListItem.NotFound
							|| firstError == Vodovoz.Errors.Orders.OrderErrors.NotFound
							|| firstError == Vodovoz.Errors.Clients.DeliveryPointErrors.NotFound)
						{
							return StatusCodes.Status404NotFound;
						}

						return StatusCodes.Status500InternalServerError;
					});
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Произошла ошибка при регистрации предположительных координат для адреса {RouteListAddressId}: {ExceptionMessage}",
					routeListAddressCoordinate.RouteListAddressId,
					ex.Message);

				resultMessage = ex.Message;

				return Problem("Произошла ошибка при регистрации предположительных координат");
			}
			finally
			{
				_driverMobileAppActionRecordService.RegisterAction(driver, DriverMobileAppActionType.OpenOrderReceiptionPanel, localActionTime, recievedTime, resultMessage);
			}
		}

		/// <summary>
		/// Регистрация координат трека
		/// </summary>
		/// <param name="registerTrackCoordinateRequestModel"></param>
		/// <returns></returns>
		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		public async Task<IActionResult> RegisterTrackCoordinatesAsync(
			[FromBody] RegisterTrackCoordinateRequest registerTrackCoordinateRequestModel)
		{
			_logger.LogInformation("Попытка регистрации треков для МЛ {RouteListId} пользователем {Username} User token: {AccessToken}",
				registerTrackCoordinateRequestModel.RouteListId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeService.GetByAPILogin(user.UserName);

			_trackPointsService.RegisterForRouteList(
				registerTrackCoordinateRequestModel.RouteListId,
				registerTrackCoordinateRequestModel.TrackList.ToList(),
				driver.Id);

			return NoContent();
		}
	}
}
