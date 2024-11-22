using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using RobotMiaApi.Contracts.Responses.V1;
using RobotMiaApi.Extensions.Mapping;
using RobotMiaApi.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.Services;

namespace RobotMiaApi.Controllers.V1
{
	/// <summary>
	/// Контроллер контрагентов
	/// </summary>
	public class CounterpartyController : VersionedController
	{
		private readonly IncomingCallCallService _incomingCallCallService;
		private readonly ICounterpartyService _counterpartyService;

		/// <summary>
		/// Констркутор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="incomingCallCallService"></param>
		/// <param name="counterpartyService"></param>
		public CounterpartyController(
			ILogger<ApiControllerBase> logger,
			IncomingCallCallService incomingCallCallService,
			ICounterpartyService counterpartyService)
			: base(logger)
		{
			_incomingCallCallService = incomingCallCallService
				?? throw new ArgumentNullException(nameof(incomingCallCallService));
			_counterpartyService = counterpartyService
				?? throw new ArgumentNullException(nameof(counterpartyService));
		}

		/// <summary>
		/// Получение контрагента по номеру телефона
		/// </summary>
		/// <param name="callId">Идентификатор звонка</param>
		/// <param name="phoneNumber">Номер телефона</param>
		/// <param name="unitOfWork"></param>
		/// <returns></returns>
		[HttpGet]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CounterpartyDto>))]
		public async Task<IActionResult> GetAsync(
			[FromQuery(Name = "call_id"), Required] Guid callId,
			[FromQuery(Name = "phone_number"), Required] string phoneNumber,
			[FromServices] IUnitOfWork unitOfWork)
		{
			phoneNumber = phoneNumber.NormalizePhone();

			var possibleCounterparties = _counterpartyService
				.GetByNormalizedPhoneNumber(unitOfWork, phoneNumber)
				.ToList();

			if(possibleCounterparties.Count != 1)
			{
				await _incomingCallCallService.RegisterCallAsync(callId, phoneNumber, null, unitOfWork);
				await unitOfWork.CommitAsync();

				return Problem("Клиент с данным телефоном не найден", statusCode: StatusCodes.Status404NotFound);
			}
			else
			{
				await _incomingCallCallService.RegisterCallAsync(callId, phoneNumber, possibleCounterparties.First().Id, unitOfWork);
				await unitOfWork.CommitAsync();
			}

			return Ok(possibleCounterparties.MapToCounterpartyDtoV1());
		}
	}
}
