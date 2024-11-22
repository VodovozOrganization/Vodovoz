using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using RobotMiaApi.Contracts.Responses.V1;
using RobotMiaApi.Extensions.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Orders;
using Vodovoz.Presentation.WebApi.Common;

namespace RobotMiaApi.Controllers.V1
{
	/// <summary>
	/// Контроллер получения причин невозврата тары
	/// </summary>
	public class TareNonReturnReasonsController : VersionedController
	{
		private readonly IGenericRepository<NonReturnReason> _nonReturnReasonRepository;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="nonReturnReasonRepository"></param>
		public TareNonReturnReasonsController(
			ILogger<ApiControllerBase> logger,
			IGenericRepository<NonReturnReason> nonReturnReasonRepository)
			: base(logger)
		{
			_nonReturnReasonRepository = nonReturnReasonRepository
				?? throw new ArgumentNullException(nameof(nonReturnReasonRepository));
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
		[ProducesResponseType(StatusCodes.Status200OK)]
		public IEnumerable<TareNonReturnReasonDto> Get(
			[FromQuery(Name = "call_id"), Required] Guid callId,
			[FromServices] IUnitOfWork unitOfWork)
		{
			var nonReturnReasons = _nonReturnReasonRepository.Get(unitOfWork);

			return nonReturnReasons.MapToTareNonReturnReasonDtoV1();
		}
	}
}
