using DriverAPI.Library.DataAccess;
using DriverAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
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
        private readonly IEmployeeRepository employeeRepository;
        private readonly IRouteListItemRepository routeListItemRepository;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IDriverMobileAppActionRecordData driverMobileAppActionRecordData;
        private readonly ITrackPointsData trackPointsData;
        private readonly IUnitOfWork unitOfWork;

        public RegistrationsController(ILogger<RegistrationsController> logger,
            IEmployeeRepository employeeRepository,
            IRouteListItemRepository routeListItemRepository,
            UserManager<IdentityUser> userManager,
            IDriverMobileAppActionRecordData driverMobileAppActionRecordData,
            ITrackPointsData trackPointsData,
            IUnitOfWork unitOfWork)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            this.routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.driverMobileAppActionRecordData = driverMobileAppActionRecordData ?? throw new ArgumentNullException(nameof(driverMobileAppActionRecordData));
            this.trackPointsData = trackPointsData ?? throw new ArgumentNullException(nameof(trackPointsData));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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
                        DriverMobileAppActionType driverMobileAppActionType;

                        switch (driverActionModel.ActionType)
                        {
                            case Library.Models.APIActionType.OpenOrderInfoPanel:
                                driverMobileAppActionType = DriverMobileAppActionType.OpenOrderInfoPanel;
                                break;
                            case Library.Models.APIActionType.OpenOrderDeliveryPanel:
                                driverMobileAppActionType = DriverMobileAppActionType.OpenOrderDeliveryPanel;
                                break;
                            case Library.Models.APIActionType.OpenOrderReceiptionPanel:
                                driverMobileAppActionType = DriverMobileAppActionType.OpenOrderReceiptionPanel;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(driverActionModel.ActionType));
                        }

                        var driverEmail = userManager.GetEmailAsync(userManager.GetUserAsync(User).Result).Result;
                        var driver = employeeRepository.GetEmployeeByEmail(unitOfWork, driverEmail);

                        driverMobileAppActionRecordData.RegisterAction(driver, driverMobileAppActionType, driverActionModel.ActionTime);
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
                var routeListAddress = routeListItemRepository.GetRouteListItemById(unitOfWork, routeListAddressCoordinate.RouteListAddressId);
                var deliveryPoint = routeListAddress?.Order?.DeliveryPoint;

                if (deliveryPoint == null)
                {
                    return BadRequest();
                }

                var coordinate = new DeliveryPointEstimatedCoordinate()
                {
                    DeliveryPointId = deliveryPoint.Id,
                    Latitude = routeListAddressCoordinate.Latitude,
                    Longitude = routeListAddressCoordinate.Longitude,
                    RegistrationTime = routeListAddressCoordinate.ActionTime
                };

                deliveryPoint.DeliveryPointEstimatedCoordinates.Add(coordinate);

                unitOfWork.Save(coordinate);
                unitOfWork.Save(deliveryPoint);
                unitOfWork.Commit();

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
