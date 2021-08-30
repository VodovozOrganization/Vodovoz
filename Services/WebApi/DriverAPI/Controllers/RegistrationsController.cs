using DriverAPI.DTOs;
using DriverAPI.Library.DTOs;
using DriverAPI.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
		private readonly int _timeout;
		private readonly int _futureTimeout;

		public RegistrationsController(
			ILogger<RegistrationsController> logger,
			UserManager<IdentityUser> userManager,
			IConfiguration configuration,
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
			_timeout = configuration.GetValue<int>("PostActionTimeTimeOut");
			_futureTimeout = configuration.GetValue<int>("FutureAtionTimeTimeOut");
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
			var recievedTime = DateTime.Now;
			var user = _userManager.GetUserAsync(User).Result;
			var driver = _employeeData.GetByAPILogin(user.UserName);

			if(routeListAddressCoordinate.ActionTime < recievedTime.AddMinutes(-_futureTimeout))
			{
				throw new InvalidTimeZoneException("Нельзя отправлять запросы из будущего! Проверьте настройки системного времени вашего телефона");
			}

			if(recievedTime - routeListAddressCoordinate.ActionTime > new TimeSpan(0, _timeout, 0))
			{
				throw new InvalidOperationException("Таймаут запроса операции");
			}

			_logger.LogInformation($"Регистрация предположительных координат точки доставки { routeListAddressCoordinate.RouteListAddressId }" +
				$" пользователем {HttpContext.User.Identity?.Name ?? "Unknown"}");

			_aPIRouteListData.RegisterCoordinateForRouteListItem(
				routeListAddressCoordinate.RouteListAddressId,
				routeListAddressCoordinate.Latitude,
				routeListAddressCoordinate.Longitude,
				routeListAddressCoordinate.ActionTime,
				driver.Id);

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
			var user = _userManager.GetUserAsync(User).Result;
			var driver = _employeeData.GetByAPILogin(user.UserName);

			if(!_aPIRouteListData.IsRouteListBelongToDriver(registerTrackCoordinateRequestModel.RouteListId, driver.Id))
			{
				_logger.LogWarning($"Водитель {HttpContext.User.Identity?.Name ?? "Unknown"} ({driver.Id})" +
					$" попытался зарегистрировать трек для МЛ {registerTrackCoordinateRequestModel.RouteListId}");
				throw new AccessViolationException("Нельзя регистрировать координаты трека к чужому МЛ");
			}

			_logger.LogInformation($"Регистрация треков для МЛ { registerTrackCoordinateRequestModel.RouteListId }" +
				$" пользователем {HttpContext.User.Identity?.Name ?? "Unknown"}");

			_trackPointsData.RegisterForRouteList(registerTrackCoordinateRequestModel.RouteListId, registerTrackCoordinateRequestModel.TrackList);
		}
	}
}
