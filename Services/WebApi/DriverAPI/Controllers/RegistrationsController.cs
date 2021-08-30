using DriverAPI.Library.Models;
using DriverAPI.Library.DTOs;
using DriverAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace DriverAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class RegistrationsController : ControllerBase
	{
		private readonly ILogger<RegistrationsController> _logger;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly IEmployeeModel _employeeData;
		private readonly IDriverMobileAppActionRecordModel _driverMobileAppActionRecordData;
		private readonly IRouteListModel _aPIRouteListData;
		private readonly ITrackPointsModel _trackPointsData;

		public RegistrationsController(
			ILogger<RegistrationsController> logger,
			UserManager<IdentityUser> userManager,
			IEmployeeModel employeeData,
			IDriverMobileAppActionRecordModel driverMobileAppActionRecordData,
			IRouteListModel aPIRouteListData,
			ITrackPointsModel trackPointsData)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
			_driverMobileAppActionRecordData = driverMobileAppActionRecordData ?? throw new ArgumentNullException(nameof(driverMobileAppActionRecordData));
			_aPIRouteListData = aPIRouteListData ?? throw new ArgumentNullException(nameof(aPIRouteListData));
			_trackPointsData = trackPointsData ?? throw new ArgumentNullException(nameof(trackPointsData));
		}

		/// <summary>
		/// Эндпоинт записи логов действий в БД
		/// </summary>
		/// <param name="driverActionModels">Список действий из лога для регистрации</param>
		/// <returns></returns>
		[HttpPost]
		[Route("/api/RegisterDriverActions")]
		public void RegisterDriverActions([FromBody] IEnumerable<DriverActionDto> driverActionModels)
		{
			_logger.LogInformation($"Регистрация действий в мобильном приложении пользователем {HttpContext.User.Identity?.Name ?? "Unknown"}");

			var user = _userManager.GetUserAsync(User).Result;
			var driver = _employeeData.GetByAPILogin(user.UserName);

			_driverMobileAppActionRecordData.RegisterActionsRangeForDriver(driver, driverActionModels);
		}

		// POST: RegisterRouteListAddressCoordinates
		[HttpPost]
		[Route("/api/RegisterRouteListAddressCoordinates")]
		public void RegisterRouteListAddressCoordinate([FromBody] RouteListAddressCoordinateDto routeListAddressCoordinate)
		{
			_logger.LogInformation($"Регистрация предположительных координат точки доставки { routeListAddressCoordinate.RouteListAddressId }" +
				$" пользователем {HttpContext.User.Identity?.Name ?? "Unknown"}");

			var user = _userManager.GetUserAsync(User).Result;
			var driver = _employeeData.GetByAPILogin(user.UserName);

			_aPIRouteListData.RegisterCoordinateForRouteListItem(
				routeListAddressCoordinate.RouteListAddressId,
				routeListAddressCoordinate.Latitude,
				routeListAddressCoordinate.Longitude,
				routeListAddressCoordinate.ActionTime);

			_driverMobileAppActionRecordData.RegisterAction(
				driver,
				new DriverActionDto()
				{
					ActionType = routeListAddressCoordinate.ActionType,
					ActionTime = routeListAddressCoordinate.ActionTime
				});
		}

		/// <summary>
		/// Эндпоинт регистрации координат трека
		/// </summary>
		/// <param name="registerTrackCoordinateRequestModel"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("/api/RegisterTrackCoordinates")]
		public void RegisterTrackCoordinates([FromBody] RegisterTrackCoordinateRequestDto registerTrackCoordinateRequestModel)
		{
			_logger.LogInformation($"Регистрация треков для МЛ { registerTrackCoordinateRequestModel.RouteListId }" +
				$" пользователем {HttpContext.User.Identity?.Name ?? "Unknown"}");

			_trackPointsData.RegisterForRouteList(registerTrackCoordinateRequestModel.RouteListId, registerTrackCoordinateRequestModel.TrackList);
		}
	}
}
