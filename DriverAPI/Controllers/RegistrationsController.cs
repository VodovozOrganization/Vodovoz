using DriverAPI.Library.Converters;
using DriverAPI.Library.DataAccess;
using DriverAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic.Drivers;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;

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
        private readonly ActionTypeConverter actionTypeConverter;

        public RegistrationsController(
            ILogger<RegistrationsController> logger,
            UserManager<IdentityUser> userManager,
            IEmployeeData employeeData,
            IDriverMobileAppActionRecordData driverMobileAppActionRecordData,
            IAPIRouteListData aPIRouteListData,
            ITrackPointsData trackPointsData,
            ActionTypeConverter actionTypeConverter)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
            this.driverMobileAppActionRecordData = driverMobileAppActionRecordData ?? throw new ArgumentNullException(nameof(driverMobileAppActionRecordData));
            this.aPIRouteListData = aPIRouteListData ?? throw new ArgumentNullException(nameof(aPIRouteListData));
            this.trackPointsData = trackPointsData ?? throw new ArgumentNullException(nameof(trackPointsData));
            this.actionTypeConverter = actionTypeConverter ?? throw new ArgumentNullException(nameof(actionTypeConverter));
        }

        /// <summary>
        /// Эндпоинт записи логов действий в БД
        /// </summary>
        /// <param name="driverActionModels">Список действий из лога для регистрации</param>
        /// <returns></returns>
        [HttpPost]
        [Route("/api/RegisterDriverActions")]
        public IActionResult RegisterDriverActions([FromBody] IEnumerable<DriverActionModel> driverActionModels)
        {
            try
            {
                foreach (var driverActionModel in driverActionModels)
                {
                    try // Должны ли регистрироваться все экшны, которые прошли валидацию?
                    {
                        var user = userManager.GetUserAsync(User).Result;
                        var driver = employeeData.GetByAPILogin(user.UserName);

                        driverMobileAppActionRecordData.RegisterAction(
                            driver,
                            actionTypeConverter.ConvertToDriverMobileAppActionType(driverActionModel.ActionType),
                            driverActionModel.ActionTime);
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        logger.LogWarning(e, $"Ошибка преобразования входящего значения: {e.ActualValue}");
                    }
                }

                return Ok();
            }
            catch (Exception e)
            {
                logger.LogWarning(e, e.Message);
                return BadRequest(e.Message);
            }
        }

        // POST: RegisterRouteListAddressCoordinates
        [HttpPost]
        [Route("/api/RegisterRouteListAddressCoordinates")]
        public IActionResult RegisterRouteListAddressCoordinate([FromBody] RouteListAddressCoordinate routeListAddressCoordinate)
        {
            try
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
                    actionTypeConverter.ConvertToDriverMobileAppActionType(routeListAddressCoordinate.ActionType),
                    routeListAddressCoordinate.ActionTime);

                return Ok();
            }
            catch (Exception e)
            {
                logger.LogWarning(e, e.Message);
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Эндпоинт регистрации координат трека
        /// </summary>
        /// <param name="registerTrackCoordinateRequestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/api/RegisterTrackCoordinates")]
        public IActionResult RegisterTrackCoordinates([FromBody] RegisterTrackCoordinateRequestModel registerTrackCoordinateRequestModel)
        {
            try
            {
                trackPointsData.RegisterForRouteList(registerTrackCoordinateRequestModel.RouteListId, registerTrackCoordinateRequestModel.TrackList);
                return Ok();
            }
            catch (Exception e)
            {
                logger.LogWarning(e, e.Message);
                return BadRequest(e.Message);
            }
        }
    }
}
