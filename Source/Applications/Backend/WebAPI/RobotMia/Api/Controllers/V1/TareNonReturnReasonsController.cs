using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Orders;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.RobotMia.Api.Extensions.Mapping;
using Vodovoz.RobotMia.Api.Services;
using Vodovoz.RobotMia.Contracts.Responses.V1;

namespace Vodovoz.RobotMia.Api.Controllers.V1
{
	/// <summary>
	/// Контроллер получения причин невозврата тары
	/// </summary>
	public class TareNonReturnReasonsController : VersionedController
	{
		private readonly IGenericRepository<NonReturnReason> _nonReturnReasonRepository;
		private readonly IIncomingCallCallService _incomingCallService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="nonReturnReasonRepository"></param>
		/// <param name="incomingCallService"></param>
		public TareNonReturnReasonsController(
			ILogger<ApiControllerBase> logger,
			IGenericRepository<NonReturnReason> nonReturnReasonRepository,
			IIncomingCallCallService incomingCallService)
			: base(logger)
		{
			_nonReturnReasonRepository = nonReturnReasonRepository
				?? throw new ArgumentNullException(nameof(nonReturnReasonRepository));
			_incomingCallService = incomingCallService
				?? throw new ArgumentNullException(nameof(incomingCallService));
		}

		/// <summary>
		/// Получение причин не возврата тары
		/// </summary>
		/// <param name="callId">Идентификатор звонка</param>
		/// <param name="unitOfWork"></param>
		/// <returns></returns>
		[HttpGet]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TareNonReturnReasonDto>))]
		public async Task<IActionResult> GetAsync(
			[FromQuery(Name = "call_id"), Required] Guid callId,
			[FromServices] IUnitOfWork unitOfWork)
		{
			var call = await _incomingCallService.GetCallByIdAsync(callId, unitOfWork);

			if(call is null)
			{
				return Problem($"Не найдена запись о звонке {callId}", statusCode: StatusCodes.Status400BadRequest);
			}

			var nonReturnReasons = _nonReturnReasonRepository.Get(unitOfWork);

			return Ok(nonReturnReasons.MapToTareNonReturnReasonDtoV1());
		}
	}
}
