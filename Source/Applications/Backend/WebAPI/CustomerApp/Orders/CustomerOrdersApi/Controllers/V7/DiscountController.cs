using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.V7.Dto.Orders;
using CustomerOrdersApi.Library.V7.Dto.Orders.Promotions.Discounts;
using CustomerOrdersApi.Library.V7.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerOrdersApi.Controllers.V7
{
	[ApiVersion("7.0")]
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
					await _discountService.GetFirstOrderDiscountConditions(
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
	}
}
