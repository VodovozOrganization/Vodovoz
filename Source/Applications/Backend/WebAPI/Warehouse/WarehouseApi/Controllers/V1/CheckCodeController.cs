using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoreLinq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.Presentation.WebApi.Security.OnlyOneSession;
using VodovozBusiness.Services.TrueMark;
using WarehouseApi.Contracts.V1.Dto;
using WarehouseApi.Filters;
using WarehouseApi.Library.Converters;

namespace WarehouseApi.Controllers.V1
{
	/// <summary>
	/// Контроллер проверки кодов
	/// </summary>
	[Authorize(Roles = _rolesToAccess)]
	[WarehouseErrorHandlingFilter]
	[OnlyOneSession]
	[Route("api/[controller]")]
	public class CheckCodeController : VersionedController
	{
		private const string _rolesToAccess =
			nameof(ApplicationUserRole.WarehousePicker) + "," + nameof(ApplicationUserRole.WarehouseDriver);
		private readonly ITrueMarkWaterCodeService _trueMarkWaterCodeService;
		private readonly CarLoadDocumentConverter _carLoadDocumentConverter;

		/// <summary>
		/// Контроллер проверки кодов
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="trueMarkWaterCodeService"></param>
		/// <param name="carLoadDocumentConverter"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public CheckCodeController(
			ILogger<ApiControllerBase> logger,
			ITrueMarkWaterCodeService trueMarkWaterCodeService,
			CarLoadDocumentConverter carLoadDocumentConverter
			) : base(logger)
		{
			_trueMarkWaterCodeService = trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
			_carLoadDocumentConverter = carLoadDocumentConverter ?? throw new ArgumentNullException(nameof(carLoadDocumentConverter));
		}


		/// <summary>
		/// Получение списка кодов ЧЗ по отсканированному коду
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="code">Отсканированный код</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список кодов ЧЗ</returns>
		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<TrueMarkCodeDto>), StatusCodes.Status200OK)]
		public async Task<IActionResult> Get(
			[FromServices] IUnitOfWork unitOfWork,
			[FromQuery] string code,
			CancellationToken cancellationToken)
		{
			try
			{
				var createCodeResult = await _trueMarkWaterCodeService.CreateStagingTrueMarkCode(
					unitOfWork,
					code,
					StagingTrueMarkCodeRelatedDocumentType.SelfDeliveryDocumentItem,
					0,
					null,
					cancellationToken);

				if(createCodeResult.IsFailure)
				{
					var error = createCodeResult.Errors.FirstOrDefault();
					_logger.LogError("Ошибка при получении кодов ЧЗ. Сообщение: {ErrorMessage}", error.Message);
					return Problem($"Ошибка при получении кодов ЧЗ: {error?.Message}", statusCode: StatusCodes.Status500InternalServerError);
				}

				var stagingCode = createCodeResult.Value;

				var isCodeUsedResult =
					await _trueMarkWaterCodeService.IsStagingTrueMarkCodeAlreadyUsedInProductCodes(unitOfWork, stagingCode, cancellationToken);

				if(isCodeUsedResult.IsFailure)
				{
					var error = isCodeUsedResult.Errors.FirstOrDefault();
					_logger.LogError("Ошибка при проверке использования кода ЧЗ. Сообщение: {ErrorMessage}", error.Message);
					return Problem($"Ошибка при проверке использования кода ЧЗ: {error?.Message}", statusCode: StatusCodes.Status400BadRequest);
				}

				var codesDto = PopulateStagingTrueMarkCode(stagingCode).ToList();

				var counter = 0;
				
				foreach(var codeDto in codesDto)
				{
					codeDto.SequenceNumber = counter++;
				}

				return Ok(codesDto);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при получении кодов ЧЗ по отсканированному коду {code}", code);
				return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
			}
		}

		private IEnumerable<TrueMarkCodeDto> PopulateStagingTrueMarkCode(StagingTrueMarkCode stagingCode, string parentCode = "")
		{
			var result = new List<TrueMarkCodeDto>();
			var level = stagingCode.CodeType switch
			{
				StagingTrueMarkCodeType.Transport => WarehouseApiTruemarkCodeLevel.transport,
				StagingTrueMarkCodeType.Group => WarehouseApiTruemarkCodeLevel.group,
				StagingTrueMarkCodeType.Identification => WarehouseApiTruemarkCodeLevel.unit,
				_ => throw new InvalidOperationException("Unknown StagingTrueMarkCodeLevel"),
			};
			var codeDto = new TrueMarkCodeDto
			{
				Code = stagingCode.RawCode,
				Level = level,
				Parent = parentCode
			};

			result.Add(codeDto);

			foreach(var innerCode in stagingCode.InnerCodes)
			{
				var childDtos = PopulateStagingTrueMarkCode(innerCode, stagingCode.RawCode);
				result.AddRange(childDtos);
			}

			return result;
		}
	}
}
