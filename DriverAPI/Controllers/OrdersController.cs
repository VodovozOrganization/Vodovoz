using DriverAPI.Library.DataAccess;
using DriverAPI.Library.Models;
using DriverAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.EntityRepositories.Employees;

namespace DriverAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> logger;
        private readonly IEmployeeData employeeData;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IAPIOrderData aPIOrderData;

        public OrdersController(ILogger<OrdersController> logger,
            IEmployeeData employeeData,
            UserManager<IdentityUser> userManager,
            IAPIOrderData aPIOrderData)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.employeeData = employeeData ?? throw new ArgumentNullException(nameof(employeeData));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.aPIOrderData = aPIOrderData ?? throw new ArgumentNullException(nameof(aPIOrderData));
        }

        /// <summary>
        /// Эндпоинт получения информации о заказе
        /// В ответе сервера будет JSON объект с полями соответствующими APIOrder и статусом 200
        /// Или пустой ответ с кодом 204
        /// </summary>
        /// <param name="orderId">Идентификатор заказа</param>
        /// <returns>APIOrder или null</returns>
        [HttpGet]
        [Route("/api/GetOrder")]
        public IActionResult Get(int orderId)
        {
            try
            {
                return Ok(aPIOrderData.Get(orderId));
            }
            catch (Exception e)
            {
                logger.LogWarning(e, e.Message);
                return BadRequest(new ErrorResponseModel(e.Message));
            }
        }

        // POST: CompleteOrderDelivery / CompleteRouteListAddress
        [HttpPost]
        [Route("/api/CompleteOrderDelivery")]
        public IActionResult CompleteOrderDelivery([FromBody] CompletedOrderRequestModel completedOrderRequestModel)
        {
            try
            {
                var user = userManager.GetUserAsync(User).Result;
                var driver = employeeData.GetByAPILogin(user.UserName);

                aPIOrderData.CompleteOrderDelivery(
                    driver,
                    completedOrderRequestModel.OrderId,
                    completedOrderRequestModel.BottlesReturnCount,
                    completedOrderRequestModel.Rating,
                    completedOrderRequestModel.DriverComplaintReasonId,
                    completedOrderRequestModel.OtherDriverComplaintReasonComment,
                    completedOrderRequestModel.ActionTime
                );

                return Ok();
            }
            catch (Exception e)
            {
                logger.LogWarning(e, e.Message);
                return BadRequest(new ErrorResponseModel(e.Message));
            }
        }

        /// <summary>
        /// Эндпоинт смены типа оплаты заказа
        /// </summary>
        /// <param name="changeOrderPaymentTypeRequestModel">Модель данных входящего запроса</param>
        /// <returns>Ответ с кодом 200 если все в порядке либо ответ с кодом 400 - ошибка</returns>
        [HttpPost]
        [Route("/api/ChangeOrderPaymentType")]
        public IActionResult ChangeOrderPaymentType(ChangeOrderPaymentTypeRequestModel changeOrderPaymentTypeRequestModel)
        {
            var orderId = changeOrderPaymentTypeRequestModel.OrderId;
            var newPaymentType = changeOrderPaymentTypeRequestModel.NewPaymentType;
            APIPaymentType newEnumPaymentType;

            if (!Enum.TryParse(newPaymentType, out newEnumPaymentType))
            {
                var error = $"Неправильный формат входных данных {nameof(changeOrderPaymentTypeRequestModel.NewPaymentType)} = '{changeOrderPaymentTypeRequestModel.NewPaymentType}'";
                logger.LogWarning(error);
                return BadRequest(new ErrorResponseModel(error));
            }

            IEnumerable<APIPaymentType> availableTypesToChange;

            try
            {
                availableTypesToChange = aPIOrderData.GetAvailableToChangePaymentTypes(orderId);
            } 
            catch (Exception e)
            {
                logger.LogWarning(e, e.Message);
                return BadRequest(new ErrorResponseModel(e.Message));
            }

            if (!availableTypesToChange.Contains(newEnumPaymentType))
            {
                var error = $"Попытка сменить тип оплаты у заказа {orderId} на недоступный для этого заказа тип оплаты {newPaymentType.ToString()}";
                logger.LogWarning(error);
                return BadRequest(new ErrorResponseModel(error));
            }

            Vodovoz.Domain.Client.PaymentType newVodovozPaymentType;

            if (newEnumPaymentType == APIPaymentType.Terminal)
            {
                newVodovozPaymentType = Vodovoz.Domain.Client.PaymentType.Terminal;
            }
            else if (newEnumPaymentType == APIPaymentType.Cash)
            {
                newVodovozPaymentType = Vodovoz.Domain.Client.PaymentType.cash;
            }
            else
            {
                var error = $"Попытка сменить тип оплаты у заказа {orderId} на не поддерживаемый для смены тип оплаты {newPaymentType.ToString()}";
                logger.LogWarning(error);
                return BadRequest(new ErrorResponseModel(error));
            }

            try
            {
                aPIOrderData.ChangeOrderPaymentType(orderId, newVodovozPaymentType);
                return Ok();
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                return BadRequest(new ErrorResponseModel(e.Message));
            }
        }
    }
}
