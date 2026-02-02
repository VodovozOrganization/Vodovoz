using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.Presentation.WebApi.Security.OnlyOneSession;
using VodovozBusiness.Services.TrueMark;
using WarehouseApi.Contracts.V1.Dto;
using WarehouseApi.Filters;

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

		/// <summary>
		/// Контроллер проверки кодов
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="trueMarkWaterCodeService"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public CheckCodeController(
			ILogger<ApiControllerBase> logger,
			ITrueMarkWaterCodeService trueMarkWaterCodeService
			) : base(logger)
		{
			_trueMarkWaterCodeService = trueMarkWaterCodeService ?? throw new ArgumentNullException(nameof(trueMarkWaterCodeService));
		}


		/// <summary>
		/// Получение списка кодов ЧЗ по отсканированному коду
		/// </summary>
		/// <param name="code">Отсканированный код</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список кодов ЧЗ</returns>
		[HttpGet]
		[ProducesResponseType(typeof(IEnumerable<TrueMarkCodeDto>), StatusCodes.Status200OK)]
		public async Task<IActionResult> Get([FromQuery] string code, CancellationToken cancellationToken)
		{
			try
			{
				var trueMarkCodesResult =
					(await _trueMarkWaterCodeService.GetTrueMarkAnyCodesByScannedCodes(new[] { code }, cancellationToken));

				if(trueMarkCodesResult.IsFailure)
				{
					var error = trueMarkCodesResult.Errors.FirstOrDefault();
					return Problem($"Ошибка при получении кодов ЧЗ: {error?.Message}", statusCode: StatusCodes.Status500InternalServerError);
				}

				var trueMarkAnyCodes = trueMarkCodesResult.Value
					.Select(x => x.Value)
					.ToList();

				var trueMarkCodes = new List<TrueMarkCodeDto>();

				foreach(var anyCode in trueMarkAnyCodes)
				{
					trueMarkCodes.Add(anyCode.Match(
						PopulateTransportCode(trueMarkAnyCodes),
						PopulateGroupCode(trueMarkAnyCodes),
						PopulateWaterCode(trueMarkAnyCodes)));
				}

				return Ok(trueMarkCodes);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при получении кодов ЧЗ по отсканированному коду {code}", code);
				return StatusCode(StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера");
			}
		}

		private static Func<TrueMarkWaterIdentificationCode, TrueMarkCodeDto> PopulateWaterCode(IEnumerable<TrueMarkAnyCode> allCodes)
		{
			return waterCode =>
			{
				string parentRawCode = null;

				if(waterCode.ParentTransportCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkTransportCode
							&& x.TrueMarkTransportCode.Id == waterCode.ParentTransportCodeId)
						?.TrueMarkTransportCode.RawCode;
				}

				if(waterCode.ParentWaterGroupCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkWaterGroupCode
							&& x.TrueMarkWaterGroupCode.Id == waterCode.ParentWaterGroupCodeId)
						?.TrueMarkWaterGroupCode.RawCode;
				}

				return new TrueMarkCodeDto
				{
					Code = waterCode.RawCode,
					Level = WarehouseApiTruemarkCodeLevel.unit,
					Parent = parentRawCode,
				};
			};
		}

		private static Func<TrueMarkWaterGroupCode, TrueMarkCodeDto> PopulateGroupCode(IEnumerable<TrueMarkAnyCode> allCodes)
		{
			return groupCode =>
			{
				string parentRawCode = null;

				if(groupCode.ParentTransportCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkTransportCode
							&& x.TrueMarkTransportCode.Id == groupCode.ParentTransportCodeId)
						?.TrueMarkTransportCode.RawCode;
				}

				if(groupCode.ParentWaterGroupCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkWaterGroupCode
							&& x.TrueMarkWaterGroupCode.Id == groupCode.ParentWaterGroupCodeId)
						?.TrueMarkWaterGroupCode.RawCode;
				}

				return new TrueMarkCodeDto
				{
					Code = groupCode.RawCode,
					Level = WarehouseApiTruemarkCodeLevel.group,
					Parent = parentRawCode
				};
			};
		}

		private static Func<TrueMarkTransportCode, TrueMarkCodeDto> PopulateTransportCode(IEnumerable<TrueMarkAnyCode> allCodes)
		{
			return transportCode =>
			{
				string parentRawCode = null;

				if(transportCode.ParentTransportCodeId != null)
				{
					parentRawCode = allCodes
						.FirstOrDefault(x => x.IsTrueMarkTransportCode
							&& x.TrueMarkTransportCode.Id == transportCode.ParentTransportCodeId)
						?.TrueMarkTransportCode.RawCode;
				}

				return new TrueMarkCodeDto
				{
					Code = transportCode.RawCode,
					Level = WarehouseApiTruemarkCodeLevel.transport,
					Parent = parentRawCode
				};
			};
		}
	}
}
