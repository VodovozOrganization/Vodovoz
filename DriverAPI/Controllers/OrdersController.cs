using DriverAPI.Library.DataAccess;
using DriverAPI.Library.Models;
using DriverAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DriverAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IAPIOrderData aPIOrderData;

        public OrdersController(IAPIOrderData aPIOrderData)
        {
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
        public APIOrder Get([FromBody] int orderId)
        {
            try
            {
                return aPIOrderData.Get(orderId);
            }
            catch (InvalidOperationException _)
            {
                return null;
            }
        }

        // POST: CompleteOrderDelivery / CompleteRouteListAddress
        [HttpPost]
        [Route("/api/CompleteOrderDelivery")]
        public IActionResult CompleteOrderDelivery([FromBody] CompletedOrderRequestModel completedOrderRequestModel)
        {
            if (true)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        // POST: ChangeOrderPaymentType
        [HttpPost]
        [Route("/api/ChangeOrderPaymentType")]
        public IActionResult ChangeOrderPaymentType(ChangeOrderPaymentTypeRequestModel changeOrderPaymentTypeRequestModel)
        {
            if (true)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
