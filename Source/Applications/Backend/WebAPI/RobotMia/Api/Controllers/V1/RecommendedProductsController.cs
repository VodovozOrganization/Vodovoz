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
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.RobotMia.Api.Services;
using Vodovoz.RobotMia.Contracts.Responses.V1;

namespace Vodovoz.RobotMia.Api.Controllers.V1
{
	/// <summary>
	/// Контроллер рекомендованных товаров
	/// </summary>
	public class RecommendedProductsController : VersionedController
	{
		private readonly IIncomingCallCallService _incomingCallService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="incomingCallService"></param>
		public RecommendedProductsController(
			ILogger<ApiControllerBase> logger,
			IIncomingCallCallService incomingCallService)
			: base(logger)
		{
			_incomingCallService = incomingCallService
				?? throw new ArgumentNullException(nameof(incomingCallService));
		}

		/// <summary>
		/// Получение списка рекомендованных товаров
		/// </summary>
		/// <param name="callId">Идентификатор звонка</param>
		/// <param name="unitOfWork"></param>
		/// <returns></returns>
		[HttpGet]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RecommendedNomenclatureDto>))]
		public async Task<IActionResult> GetAsync(
			[FromQuery(Name = "call_id"), Required] Guid callId,
			[FromServices] IUnitOfWork unitOfWork)
		{
			var call = await _incomingCallService.GetCallByIdAsync(callId, unitOfWork);

			if(call is null)
			{
				return Problem($"Не найдена запись о звонке {callId}", statusCode: StatusCodes.Status400BadRequest);
			}

			return Ok(Enumerable.Empty<RecommendedNomenclatureDto>());
		}
	}
}
