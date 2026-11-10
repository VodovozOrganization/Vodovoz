using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.V8.Dto.Orders;
using CustomerOrdersApi.Library.V8.Dto.Orders.Promotions.Discounts;
using CustomerOrdersApi.Library.V8.Dto.Sale;
using CustomerOrdersApi.Library.V8.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerOrdersApi.Controllers.V8
{
	[ApiVersion("8.0")]
	public class DiscountController : SignatureControllerBase
	{
		private readonly ICustomerOrdersDiscountService _discountService;
		private readonly IBonusToOrderService _bonusToOrderService;

		public DiscountController(
			ILogger<SignatureControllerBase> logger,
			ICustomerOrdersDiscountService discountService,
			IBonusToOrderService bonusToOrderService
			) : base(logger)
		{
			_discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
			_bonusToOrderService = bonusToOrderService ?? throw new ArgumentNullException(nameof(bonusToOrderService));
		}

		[HttpGet]
		public IActionResult ApplyPromoCodeToOrder([FromBody] ApplyPromoCodeDto applyPromoCodeDto)
		{
			var sourceName = applyPromoCodeDto.Source.GetEnumTitle();

			try
			{
				_logger.LogInformation("Поступил запрос на применение промокода {@PromoCodeRequest}, проверяем...", applyPromoCodeDto);

				if(!_discountService.ValidateApplyingPromoCodeSignature(applyPromoCodeDto, out var generatedSignature))
				{
					return InvalidSignature(applyPromoCodeDto.Signature, generatedSignature);
				}

				_logger.LogInformation("Подпись валидна, применяем промокод {PromoCode}", applyPromoCodeDto.PromoCode);
				var result = _discountService.ApplyPromoCodeToOnlineOrder(applyPromoCodeDto);

				_logger.LogInformation("Отправляем ответ по промокоду: {@PromoCodeResponse}", result);
				return Ok(result);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при применении промокода {Promocode} для заказа {ExternalOrderId}" +
					" пользователя {ExternalClientId} от {Source}",
					applyPromoCodeDto.PromoCode,
					applyPromoCodeDto.ExternalOrderId,
					applyPromoCodeDto.ExternalCounterpartyId,
					sourceName);

				return Problem();
			}
		}

		/// <summary>
		/// Применение скидки на первый заказ
		/// </summary>
		[HttpGet]
		[Authorize]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AppliedFirstOrderDiscountDto))]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> ApplyFirstOrderDiscount(
			[FromBody] ApplyFirstOrderDiscountDto applyFirstOrderDiscountDto,
			CancellationToken cancellationToken)
		{
			var sourceName = applyFirstOrderDiscountDto.Source.GetEnumTitle();

			try
			{
				_logger.LogInformation(
					"Поступил запрос на применение скидки на первый заказ {@FirstOrderDiscountRequest}, проверяем...", applyFirstOrderDiscountDto);

				var result =
					await _discountService.ApplyFirstOrderDiscount(applyFirstOrderDiscountDto, cancellationToken);

				_logger.LogInformation("Отправляем ответ по скидке: {@FirstOrderDiscountResponse}", result);
				return Ok(result);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при применении скидки на первый заказ {ExternalOrderId} пользователя {ExternalClientId} от {Source}",
					applyFirstOrderDiscountDto.ExternalOrderId,
					applyFirstOrderDiscountDto.ExternalCounterpartyId,
					sourceName);

				return Problem();
			}
		}

		[HttpGet]
		public IActionResult GetPromoCodeWarningMessage([FromBody] PromoCodeWarningDto promoCodeWarningDto)
		{
			var sourceName = promoCodeWarningDto.Source.GetEnumTitle();

			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} на оповещение пользователя о применимости промокода {PromoCode}" +
					" для заказа {ExternalOrderId} c подписью {Signature}, проверяем...",
					sourceName,
					promoCodeWarningDto.PromoCode,
					promoCodeWarningDto.ExternalOrderId,
					promoCodeWarningDto.Signature);

				if(!_discountService.ValidatePromoCodeWarningSignature(promoCodeWarningDto, out var generatedSignature))
				{
					return InvalidSignature(promoCodeWarningDto.Signature, generatedSignature);
				}

				var message =
					$"Вы ввели промокод {promoCodeWarningDto.PromoCode}. " +
					"Скидки не суммируются, при возможности будет применена максимальная из них";

				_logger.LogInformation("Подпись валидна, отправляем сообщение...");
				return Ok(message);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при оповещении пользователя о применимости промокода {Promocode} для заказа {ExternalOrderId} от {Source}",
					promoCodeWarningDto.PromoCode,
					promoCodeWarningDto.ExternalOrderId,
					sourceName);

				return Problem();
			}
		}

		/// <summary>
		/// Проверка доступности использования скидки на первый заказ для клиента
		/// </summary>
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FirstOrderDiscountConditionsDto))]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetFirstOrderDiscountConditions(
			[FromBody] FirstOrderDiscountConditionsRequestDto requestDto,
			CancellationToken cancellationToken
		)
		{
			var sourceName = requestDto.Source.GetEnumTitle();

			try
			{
				_logger.LogInformation(
					"Поступил запрос доступности использования скидки на первый заказ для клиента {@FirstOrderDiscountConditionsRequest}, проверяем...",
					requestDto);

				var result =
					await _discountService.CanApplyFirstOrderDiscount(
						requestDto.Source,
						requestDto.ExternalCounterpartyId,
						requestDto.ErpCounterpartyId,
						cancellationToken
					);

				return Ok(result);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при проверке доступности использования скидки на первый заказ для клиента " +
					"ExternalCounterpartyId = {ExternalClientId}, CounterpartyErpId = {CounterpartyErpId} от {Source}",
					requestDto.ExternalCounterpartyId,
					requestDto.ErpCounterpartyId,
					sourceName
				);

				return Problem();
			}
		}

		/// <summary>
		/// Расчёт баланса, доступного списания и начисления бонусов при оформлении заказа
		/// </summary>
		[HttpPost]
		[Authorize]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplyBonusToOrderResponse))]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public Task<IActionResult> ApplyBonusToOrder(
			[FromBody] ApplyBonusToOrderRequest applyBonusToOrder,
			CancellationToken cancellationToken)
		{
			return CalculateBonusAsync(applyBonusToOrder, cancellationToken);
		}

		private async Task<IActionResult> CalculateBonusAsync(
			ApplyBonusToOrderRequest applyBonusToOrder,
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
