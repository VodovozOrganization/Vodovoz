using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.RobotMia.Api.Services;
using Vodovoz.RobotMia.Contracts.Responses.V1;

namespace Vodovoz.RobotMia.Api.Controllers.V1
{
	/// <summary>
	/// Контроллер информации о бутылях на возврат
	/// </summary>
	public class BottlesForReturnFromDeliveryPointController : VersionedController
	{
		private readonly IBottlesRepository _bottlesRepository;
		private readonly IIncomingCallCallService _incomingCallService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="incomingCallService"></param>
		/// <param name="bottlesRepository"></param>
		public BottlesForReturnFromDeliveryPointController(
			ILogger<ApiControllerBase> logger,
			IIncomingCallCallService incomingCallService,
			IBottlesRepository bottlesRepository)
			: base(logger)
		{
			_incomingCallService = incomingCallService
				?? throw new ArgumentNullException(nameof(incomingCallService));
			_bottlesRepository = bottlesRepository
				?? throw new ArgumentNullException(nameof(bottlesRepository));
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
			var call = await _incomingCallService.GetCallByIdAsync(callId, unitOfWork);

			if(call is null)
			{
				return Problem($"Не найдена запись о звонке {callId}", statusCode: StatusCodes.Status400BadRequest);
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
