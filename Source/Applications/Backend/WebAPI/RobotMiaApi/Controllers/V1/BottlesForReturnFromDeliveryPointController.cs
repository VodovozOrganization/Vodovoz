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
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.Presentation.WebApi.Common;

namespace RobotMiaApi.Controllers.V1
{
	/// <summary>
	/// Контроллер информации о бутылях на возврат
	/// </summary>
	public class BottlesForReturnFromDeliveryPointController : VersionedController
	{
		private readonly IBottlesRepository _bottlesRepository;
		private readonly IncomingCallCallService _incomingCallCallService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="incomingCallCallService"></param>
		public BottlesForReturnFromDeliveryPointController(
			ILogger<ApiControllerBase> logger,
			IncomingCallCallService incomingCallCallService)
			: base(logger)
		{
			_incomingCallCallService = incomingCallCallService
				?? throw new ArgumentNullException(nameof(incomingCallCallService));
		}

		/// <summary>
		/// Запрос количества бутылей, ожидаемых к возврату с адреса
		/// </summary>
		/// <param name="callId">Идентификатор звонка</param>
		/// <param name="deliveryPointId">Идентификатор точки доставки</param>
		/// <param name="unitOfWork"></param>
		/// <returns></returns>
		[HttpGet]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BottlesForReturnFromDeliveryPointResponse))]
		public async Task<IActionResult> GetAsync(
			[FromQuery(Name = "call_id"), Required] Guid callId,
			[FromQuery(Name = "delivery_point_id"), Required] int deliveryPointId,
			[FromServices] IUnitOfWork unitOfWork)
		{
			var call = await _incomingCallCallService.GetCallByIdAsync(callId, unitOfWork);

			if(call is null)
			{
				return Problem("Не найдено записи о звонке", statusCode: StatusCodes.Status400BadRequest);
			}

			if(call.CounterpartyId is null)
			{
				return Problem("Не найден контрагент", statusCode: StatusCodes.Status400BadRequest);
			}

			var bottlesAtDeliveryPoint = _bottlesRepository.GetBottlesDebtAtDeliveryPoint(unitOfWork, deliveryPointId);
			var bottlesAtCounterparty = _bottlesRepository.GetBottlesDebtAtCounterparty(unitOfWork, call.CounterpartyId.Value);

			return Ok( new BottlesForReturnFromDeliveryPointResponse
			{
				BottlesAtCouterparty = bottlesAtCounterparty,
				BottlesAtDeliveryPoint = bottlesAtDeliveryPoint
			});
		}
	}
}
