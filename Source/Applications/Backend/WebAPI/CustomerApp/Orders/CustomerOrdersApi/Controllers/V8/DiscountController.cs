using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.V8.Dto.Sale;
using CustomerOrdersApi.Library.V8.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerOrdersApi.Controllers.V8
{
	[ApiVersion("8.0")]
	public class DiscountController : SignatureControllerBase
	{
		private readonly IBonusToOrderService _bonusToOrderService;

		public DiscountController(
			ILogger<SignatureControllerBase> logger,
			IBonusToOrderService bonusToOrderService
			) : base(logger)
		{
			_bonusToOrderService = bonusToOrderService ?? throw new ArgumentNullException(nameof(bonusToOrderService));
		}

		/// <summary>
		/// Расчёт баланса, доступного списания и начисления бонусов при оформлении заказа
		/// </summary>
		/// <param name="applyBonusToOrder">Данные корзины и сумма списания</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>
		/// Данные по бонусам и позиции заказа после списания.
		/// При недоступности программы или сбое Mindbox возвращается 200 с isAvailable = false
		/// </returns>
		[HttpGet]
		[Authorize]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplyBonusToOrderResponse))]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> ApplyBonusToOrder(
			[FromBody] ApplyBonusToOrderRequest applyBonusToOrder,
			CancellationToken cancellationToken)
		{
			if(applyBonusToOrder is null)
			{
				_logger.LogWarning("Поступил пустой запрос расчёта бонусов");

				return BadRequest(new ProblemDetails
				{
					Status = StatusCodes.Status400BadRequest,
					Detail = "Некорректный запрос"
				});
			}

			try
			{
				_logger.LogInformation(
					"Поступил запрос на расчёт бонусов по заказу {ExternalOrderId} клиента {ErpCounterpartyId}, сумма списания {BonusesToWriteOff}",
					applyBonusToOrder.ExternalOrderId,
					applyBonusToOrder.ErpCounterpartyId,
					applyBonusToOrder.BonusesToWriteOff);

				var result = await _bonusToOrderService.CalculateAsync(applyBonusToOrder, cancellationToken);

				_logger.LogInformation(
					"Отправляем ответ по бонусам заказа {ExternalOrderId}: isAvailable={IsAvailable}, bonusesWrittenOff={BonusesWrittenOff}",
					applyBonusToOrder.ExternalOrderId,
					result.BonusInfo.IsAvailable,
					result.BonusInfo.BonusesWrittenOff);

				return Ok(result);
			}
			catch(BonusToOrderValidationException e)
			{
				_logger.LogWarning(
					"Ошибка валидации запроса расчёта бонусов заказа {ExternalOrderId}: {Message}",
					applyBonusToOrder.ExternalOrderId,
					e.Message);

				return BadRequest(new ProblemDetails
				{
					Status = StatusCodes.Status400BadRequest,
					Detail = e.Message
				});
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при расчёте бонусов заказа {ExternalOrderId} клиента {ErpCounterpartyId}",
					applyBonusToOrder.ExternalOrderId,
					applyBonusToOrder.ErpCounterpartyId);

				return StatusCode(
					StatusCodes.Status500InternalServerError,
					new ProblemDetails
					{
						Status = StatusCodes.Status500InternalServerError,
						Detail = "Произошла ошибка, пожалуйста, попробуйте позже"
					});
			}
		}
	}
}
