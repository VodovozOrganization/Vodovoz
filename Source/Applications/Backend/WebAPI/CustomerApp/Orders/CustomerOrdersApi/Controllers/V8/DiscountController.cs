using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using CustomerApp.Contracts.Common;
using CustomerOrders.Abstractions;
using CustomerOrders.Contracts.V8.Sale;
using CustomerOrdersApi.Library.V8.Dto.Orders;
using CustomerOrdersApi.Library.V8.Dto.Orders.Promotions.Discounts;
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

		public DiscountController(
			ILogger<SignatureControllerBase> logger,
			ICustomerOrdersDiscountService discountService
			) : base(logger)
		{
			_discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
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
		/// <param name="applyFirstOrderDiscountDto">Данные для применения скидки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список товаров с детализацией скидок, если скидка недоступна - вернется пришедший список</returns>
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
		/// <param name="requestDto">Данные клиента и источника запроса <see cref="FirstOrderDiscountConditionsRequestDto"/></param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат проверки <see cref="FirstOrderDiscountConditionsDto"/></returns>
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
		/// Применение скидки за авто заказ
		/// </summary>
		/// <param name="applyAutoOrderDiscount">Данные для применения скидки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список товаров с детализацией скидок, если скидка недоступна - вернется пришедший список</returns>
		[HttpGet]
		[Authorize]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApplyAutoOrderDiscountResponse))]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public IActionResult ApplyAutoOrderDiscount(
			[FromBody] ApplyAutoOrderDiscountRequest applyAutoOrderDiscount,
			CancellationToken cancellationToken)
		{
			var sourceName = applyAutoOrderDiscount.Source.GetEnumTitle();
			
			try
			{
				_logger.LogInformation(
					"Поступил запрос на применение скидки за автозаказ {@AutoOrderDiscountRequest}, проверяем...", applyAutoOrderDiscount);
				
				var result = _discountService.ProcessAutoOrderDiscount(applyAutoOrderDiscount, cancellationToken);

				_logger.LogInformation("Отправляем ответ по скидке: {@AutoOrderDiscountResponse}", result);
				return Ok(result);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при применении скидки за автозаказ {ExternalOrderId} пользователя {ExternalClientId} от {Source}",
					applyAutoOrderDiscount.ExternalOrderId,
					applyAutoOrderDiscount.ExternalCounterpartyId,
					sourceName);

				return Problem();
			}
		}

		/// <summary>
		/// Проверка доступности использования скидки на первый заказ для клиента
		/// </summary>
		/// <param name="source">Источник запроса <see cref="ExternalSource"/></param>
		/// <returns>Результат проверки <see cref="FirstOrderDiscountConditionsDto"/></returns>
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FirstOrderDiscountConditionsDto))]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[HttpGet]
		[Authorize]
		public IActionResult GetOrderTemplateConditions(ExternalSource source)
		{
			var sourceName = source.GetEnumTitle();

			try
			{
				_logger.LogInformation("Поступил запрос получения скидки за автозаказ от {Source}", sourceName);
				var result = _discountService.GetAutoOrderDiscount();
				return Ok(result);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при запросе получения скидки за автозаказ от {Source}",
					sourceName
				);

				return Problem();
			}
		}
	}
}
