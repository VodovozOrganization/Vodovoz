using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RobotMiaApi.Contracts.Responses.V1;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using Vodovoz.Presentation.WebApi.Common;

namespace RobotMiaApi.Controllers.V1
{
	/// <summary>
	/// Контроллер рекомендованных товаров
	/// </summary>
	public class RecommendedProductsController : VersionedController
	{
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		public RecommendedProductsController(ILogger<ApiControllerBase> logger)
			: base(logger)
		{
		}

		/// <summary>
		/// Получение списка рекомендованных товаров
		/// </summary>
		/// <param name="callId">Идентификатор звонка</param>
		/// <returns></returns>
		[HttpGet]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public IEnumerable<RecommendedNomenclatureDto> Get(
			[FromQuery(Name = "call_id"), Required] Guid callId)
		{
			return Enumerable.Empty<RecommendedNomenclatureDto>();
		}
	}
}
