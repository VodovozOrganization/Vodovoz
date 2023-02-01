﻿using DriverAPI.DTOs;
using DriverAPI.Library.DTOs;
using DriverAPI.Library.Helpers;
using DriverAPI.Library.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Net.Http.Headers;
using Vodovoz.Domain.Logistic.Drivers;
using System.Threading;
using System.Threading.Tasks;

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
		private readonly IDriverMobileAppActionRecordModel _driverMobileAppActionRecordModel;
		private readonly IRouteListModel _aPIRouteListData;
		private readonly ITrackPointsModel _trackPointsData;
		private readonly IActionTimeHelper _actionTimeHelper;

		public RegistrationsController(
			ILogger<RegistrationsController> logger,
			UserManager<IdentityUser> userManager,
			IEmployeeModel employeeData,
			IDriverMobileAppActionRecordModel driverMobileAppActionRecordModel,
			IRouteListModel aPIRouteListData,
			ITrackPointsModel trackPointsData,
			IActionTimeHelper actionTimeHelper)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			_employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
			_driverMobileAppActionRecordModel = driverMobileAppActionRecordModel
				?? throw new ArgumentNullException(nameof(driverMobileAppActionRecordModel));
			_aPIRouteListData = aPIRouteListData ?? throw new ArgumentNullException(nameof(aPIRouteListData));
			_trackPointsData = trackPointsData ?? throw new ArgumentNullException(nameof(trackPointsData));
			_actionTimeHelper = actionTimeHelper ?? throw new ArgumentNullException(nameof(actionTimeHelper));
		}

		/// <summary>
		/// Эндпоинт записи логов действий в БД
		/// </summary>
		/// <param name="driverActionModels">Список действий из лога для регистрации</param>
		/// <returns></returns>
		[HttpPost]
		[Route("/api/RegisterDriverActions")]
		public async Task RegisterDriverActionsAsync([FromBody] IEnumerable<DriverActionDto> driverActionModels)
		{

		}

		// POST: RegisterRouteListAddressCoordinates
		[HttpPost]
		[Route("/api/RegisterRouteListAddressCoordinates")]
		public async Task RegisterRouteListAddressCoordinateAsync([FromBody] RouteListAddressCoordinateDto routeListAddressCoordinate)
		{
			_logger.LogInformation("Попытка регистрации предположительных координат для адреса {RouteListAddressId} пользователем {Username} User token: {AccessToken}",
				routeListAddressCoordinate.RouteListAddressId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var recievedTime = DateTime.Now;

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeData.GetByAPILogin(user.UserName);

			var resultMessage = "OK";

			try
			{
				_actionTimeHelper.ThrowIfNotValid(recievedTime, routeListAddressCoordinate.ActionTime);

				_aPIRouteListData.RegisterCoordinateForRouteListItem(
					routeListAddressCoordinate.RouteListAddressId,
					routeListAddressCoordinate.Latitude,
					routeListAddressCoordinate.Longitude,
					routeListAddressCoordinate.ActionTime,
					driver.Id);
			}
			catch(Exception ex)
			{
				resultMessage = ex.Message;
				throw;
			}
			finally
			{
				_driverMobileAppActionRecordModel.RegisterAction(driver, DriverMobileAppActionType.OpenOrderReceiptionPanel, routeListAddressCoordinate.ActionTime, recievedTime, resultMessage);
			}
		}

		/// <summary>
		/// Эндпоинт регистрации координат трека
		/// </summary>
		/// <param name="registerTrackCoordinateRequestModel"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("/api/RegisterTrackCoordinates")]
		public async Task RegisterTrackCoordinatesAsync([FromBody] RegisterTrackCoordinateRequestDto registerTrackCoordinateRequestModel)
		{
			_logger.LogInformation("Попытка регистрации треков для МЛ {RouteListId} пользователем {Username} User token: {AccessToken}",
				registerTrackCoordinateRequestModel.RouteListId,
				HttpContext.User.Identity?.Name ?? "Unknown",
				Request.Headers[HeaderNames.Authorization]);

			var user = await _userManager.GetUserAsync(User);
			var driver = _employeeData.GetByAPILogin(user.UserName);

			_trackPointsData.RegisterForRouteList(
				registerTrackCoordinateRequestModel.RouteListId,
				registerTrackCoordinateRequestModel.TrackList.ToList(),
				driver.Id);
		}
	}
}
