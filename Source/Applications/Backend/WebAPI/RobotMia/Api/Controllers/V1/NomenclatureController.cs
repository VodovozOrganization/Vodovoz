using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Vodovoz.RobotMia.Api.Services;
using Vodovoz.RobotMia.Contracts.Responses.V1;

namespace Vodovoz.RobotMia.Api.Controllers.V1
{
	/// <summary>
	/// Контроллер номенклатур
	/// </summary>
	public class NomenclatureController : VersionedController
	{
		private readonly INomenclatureService _nomenclatureService;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="nomenclatureService"></param>
		public NomenclatureController(
			ILogger<NomenclatureController> logger,
			INomenclatureService nomenclatureService) : base(logger)
		{
			_nomenclatureService = nomenclatureService
				?? throw new ArgumentNullException(nameof(nomenclatureService));
		}

		/// <summary>
		/// Получение номенклатур
		/// </summary>
		/// <returns><see cref="IEnumerable{NomenclatureDto}"/></returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<NomenclatureDto>))]
		public async Task<IActionResult> GetAsync()
		{
			return Ok(await _nomenclatureService.GetNomenclatures());
		}

		/// <summary>
		/// Получение номенклатуры неустойки
		/// </summary>
		/// <returns><see cref="NomenclatureDto"/></returns>
		[HttpGet("Forfeit")]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NomenclatureDto))]
		public async Task<IActionResult> GetForfeitAsync()
		{
			return Ok(await _nomenclatureService.GetForfeitNomenclature());
		}
	}
}
