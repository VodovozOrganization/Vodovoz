using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.RobotMia.Api.Services;
using Vodovoz.RobotMia.Contracts.Responses.V1;

namespace Vodovoz.RobotMia.Api.Controllers.V1
{
	/// <summary>
	/// Контроллер последних заказов
	/// </summary>
	public class LastOrderController : VersionedController
	{
		private readonly IIncomingCallCallService _incomingCallService;
		private readonly IOrderService _orderService;

		/// <summary>
		/// Констркутор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="incomingCallService"></param>
		/// <param name="orderService"></param>
		public LastOrderController(
			ILogger<ApiControllerBase> logger,
			IIncomingCallCallService incomingCallService,
			IOrderService orderService)
			: base(logger)
		{
			_incomingCallService = incomingCallService
				?? throw new ArgumentNullException(nameof(incomingCallService));
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
			unitOfWork.Session.DefaultReadOnly = true;

			var call = await _incomingCallService.GetCallByIdAsync(callId, unitOfWork);

			if(call is null)
			{
				return Problem($"Не найдена запись о звонке {callId}", statusCode: StatusCodes.Status400BadRequest);
			}

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
