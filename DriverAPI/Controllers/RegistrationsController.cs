using DriverAPI.Library.DataAccess;
using DriverAPI.Library.Models;
using DriverAPI.Models;
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
		private readonly ILogger<RegistrationsController> logger;
		private readonly UserManager<IdentityUser> userManager;
		private readonly IEmployeeData employeeData;
		private readonly IDriverMobileAppActionRecordData driverMobileAppActionRecordData;
		private readonly IAPIRouteListData aPIRouteListData;
		private readonly ITrackPointsData trackPointsData;

		public RegistrationsController(
			ILogger<RegistrationsController> logger,
			UserManager<IdentityUser> userManager,
			IEmployeeData employeeData,
			IDriverMobileAppActionRecordData driverMobileAppActionRecordData,
			IAPIRouteListData aPIRouteListData,
			ITrackPointsData trackPointsData)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			this.employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
			this.driverMobileAppActionRecordData = driverMobileAppActionRecordData ?? throw new ArgumentNullException(nameof(driverMobileAppActionRecordData));
			this.aPIRouteListData = aPIRouteListData ?? throw new ArgumentNullException(nameof(aPIRouteListData));
			this.trackPointsData = trackPointsData ?? throw new ArgumentNullException(nameof(trackPointsData));
		}

		/// <summary>
		/// Эндпоинт записи логов действий в БД
		/// </summary>
		/// <param name="driverActionModels">Список действий из лога для регистрации</param>
		/// <returns></returns>
		[HttpPost]
		[Route("/api/RegisterDriverActions")]
		public void RegisterDriverActions([FromBody] IEnumerable<APIDriverActionModel> driverActionModels)
		{
			var user = userManager.GetUserAsync(User).Result;
			var driver = employeeData.GetByAPILogin(user.UserName);

			driverMobileAppActionRecordData.RegisterActionsRangeForDriver(driver, driverActionModels);
		}

		// POST: RegisterRouteListAddressCoordinates
		[HttpPost]
		[Route("/api/RegisterRouteListAddressCoordinates")]
		public void RegisterRouteListAddressCoordinate([FromBody] RouteListAddressCoordinate routeListAddressCoordinate)
		{
			var user = userManager.GetUserAsync(User).Result;
			var driver = employeeData.GetByAPILogin(user.UserName);

			aPIRouteListData.RegisterCoordinateForRouteListItem(
				routeListAddressCoordinate.RouteListAddressId,
				routeListAddressCoordinate.Latitude,
				routeListAddressCoordinate.Longitude,
				routeListAddressCoordinate.ActionTime);

			driverMobileAppActionRecordData.RegisterAction(
				driver,
				new APIDriverActionModel()
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
		public void RegisterTrackCoordinates([FromBody] RegisterTrackCoordinateRequestModel registerTrackCoordinateRequestModel)
		{
			trackPointsData.RegisterForRouteList(registerTrackCoordinateRequestModel.RouteListId, registerTrackCoordinateRequestModel.TrackList);
		}
	}
}
