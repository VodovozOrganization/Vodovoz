using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using RobotMiaApi.Contracts.Responses.V1;
using RobotMiaApi.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.Presentation.WebApi.Common;

namespace RobotMiaApi.Controllers.V1
{
	/// <summary>
	/// Контроллер последних заказов
	/// </summary>
	public class LastOrderController : VersionedController
	{
		private readonly IncomingCallCallService _incomingCallCallService;
		private readonly OrderService _orderService;

		/// <summary>
		/// Констркутор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="incomingCallCallService"></param>
		/// <param name="orderService"></param>
		public LastOrderController(
			ILogger<ApiControllerBase> logger,
			IncomingCallCallService incomingCallCallService,
			OrderService orderService)
			: base(logger)
		{
			_incomingCallCallService = incomingCallCallService
				?? throw new ArgumentNullException(nameof(incomingCallCallService));
			_orderService = orderService
				?? throw new ArgumentNullException(nameof(orderService));
		}

		/// <summary>
		/// Получение последнего заказа
		/// </summary>
		/// <param name="callId">Идентификатор звонка</param>
		/// <param name="deliveryPointId">Идентификатор точки доставки</param>
		/// <param name="unitOfWork"></param>
		/// <returns></returns>
		[HttpGet]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LastOrderResponse))]
		public async Task<IActionResult> GetAsync(
			[FromQuery(Name = "call_id"), Required] Guid callId,
			[FromQuery(Name = "delivery_point_id")] int? deliveryPointId,
			[FromServices] IUnitOfWork unitOfWork)
		{
			var call = await _incomingCallCallService.GetCallByIdAsync(callId, unitOfWork);

			if(call.CounterpartyId is null)
			{
				return Problem("Контрагент не найден", statusCode: StatusCodes.Status400BadRequest);
			}

			LastOrderResponse result;

			if(deliveryPointId.HasValue)
			{
				result = _orderService.GetLastOrderByDeliveryPointId(deliveryPointId.Value);
			}
			else
			{
				result = _orderService.GetLastOrderByCounterpartyId(call.CounterpartyId.Value);
			}

			if(result is null)
			{
				return Problem("Нет последних заказов", statusCode: StatusCodes.Status404NotFound);
			}

			return Ok(result);
		}
	}
}
