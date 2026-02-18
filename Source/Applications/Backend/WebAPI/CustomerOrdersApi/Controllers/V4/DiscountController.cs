using System;
using System.Linq;
using CustomerOrdersApi.Library.V4.Dto.Orders;
using CustomerOrdersApi.Library.V4.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerOrdersApi.Controllers.V4
{
	public class DiscountController : SignatureControllerBase
	{
		private readonly ICustomerOrdersDiscountServiceV4 _discountService;

		public DiscountController(
			ILogger<SignatureControllerBase> logger,
			ICustomerOrdersDiscountServiceV4 discountService
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

				if(result.IsSuccess)
				{
					_logger.LogInformation("Отправляем ответ по промокоду: {@PromoCodeResponse}", result.Value);
					return Ok(result.Value);
				}

				var notFoundResult = result.Errors.First().Message;
				_logger.LogWarning("Промокод {PromoCode}: {PromoCodeNotFound}", applyPromoCodeDto.PromoCode, notFoundResult);
				return NotFound(notFoundResult);
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
	}
}
