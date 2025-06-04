using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.RobotMia.Api.Extensions.Mapping;
using Vodovoz.RobotMia.Api.Services;
using Vodovoz.RobotMia.Contracts.Responses.V1;
using Vodovoz.Services;

namespace Vodovoz.RobotMia.Api.Controllers.V1
{
	/// <summary>
	/// Контроллер контрагентов
	/// </summary>
	public class CounterpartyController : VersionedController
	{
		private readonly IIncomingCallCallService _incomingCallService;
		private readonly ICounterpartyService _counterpartyService;

		/// <summary>
		/// Констркутор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="incomingCallService"></param>
		/// <param name="counterpartyService"></param>
		public CounterpartyController(
			ILogger<ApiControllerBase> logger,
			IIncomingCallCallService incomingCallService,
			ICounterpartyService counterpartyService)
			: base(logger)
		{
			_incomingCallService = incomingCallService
				?? throw new ArgumentNullException(nameof(incomingCallService));
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
				await _incomingCallService.RegisterCallAsync(callId, phoneNumber, null, unitOfWork);
				await unitOfWork.CommitAsync();

				return Problem("Клиент с данным телефоном не найден", statusCode: StatusCodes.Status404NotFound);
			}
			else
			{
				await _incomingCallService.RegisterCallAsync(callId, phoneNumber, possibleCounterparties.First().Id, unitOfWork);
				await unitOfWork.CommitAsync();
			}

			return Ok(possibleCounterparties.MapToCounterpartyDtoV1());
		}
	}
}
