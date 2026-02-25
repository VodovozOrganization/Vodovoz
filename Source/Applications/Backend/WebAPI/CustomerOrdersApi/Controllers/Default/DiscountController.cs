using System;
using System.Linq;
using CustomerOrdersApi.Library.Default.Dto.Orders;
using CustomerOrdersApi.Library.Default.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerOrdersApi.Controllers.Default
{
	[ApiVersion("3.0")]
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
				Logger.LogInformation("Поступил запрос на применение промокода {@PromoCodeRequest}, проверяем...", applyPromoCodeDto);

				if(!_discountService.ValidateApplyingPromoCodeSignature(applyPromoCodeDto, out var generatedSignature))
				{
					return InvalidSignature(applyPromoCodeDto.Signature, generatedSignature);
				}

				Logger.LogInformation("Подпись валидна, применяем промокод {PromoCode}", applyPromoCodeDto.PromoCode);
				var result = _discountService.ApplyPromoCodeToOnlineOrder(applyPromoCodeDto);

				if(result.IsSuccess)
				{
					Logger.LogInformation("Отправляем ответ по промокоду: {@PromoCodeResponse}", result.Value);
					return Ok(result.Value);
				}

				var notFoundResult = result.Errors.First().Message;
				Logger.LogWarning("Промокод {PromoCode}: {PromoCodeNotFound}", applyPromoCodeDto.PromoCode, notFoundResult);
				return NotFound(notFoundResult);
			}
			catch(Exception e)
			{
				Logger.LogError(e,
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
				Logger.LogInformation(
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

				Logger.LogInformation("Подпись валидна, отправляем сообщение...");
				return Ok(message);
			}
			catch(Exception e)
			{
				Logger.LogError(e,
					"Ошибка при оповещении пользователя о применимости промокода {Promocode} для заказа {ExternalOrderId} от {Source}",
					promoCodeWarningDto.PromoCode,
					promoCodeWarningDto.ExternalOrderId,
					sourceName);

				return Problem();
			}
		}
	}
}
