﻿using System;
using System.Linq;
using CustomerOrdersApi.Library.Dto.Orders;
using CustomerOrdersApi.Library.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerOrdersApi.Controllers
{
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
				Logger.LogInformation(
					"Поступил запрос от {Source} на применение промокода {PromoCode} для заказа {ExternalOrderId} c подписью {Signature}, проверяем...",
					sourceName,
					applyPromoCodeDto.PromoCode,
					applyPromoCodeDto.ExternalOrderId,
					applyPromoCodeDto.Signature);
				
				if(!_discountService.ValidateApplyingPromoCodeSignature(applyPromoCodeDto, out var generatedSignature))
				{
					return InvalidSignature(applyPromoCodeDto.Signature, generatedSignature);
				}

				Logger.LogInformation("Подпись валидна, применяем промокод {PromoCode}", applyPromoCodeDto.PromoCode);
				var result = _discountService.ApplyPromoCodeToOnlineOrder(applyPromoCodeDto);

				if(result.IsSuccess)
				{
					return Ok(result.Value);
				}

				return BadRequest(result.Errors.First());
			}
			catch(Exception e)
			{
				Logger.LogError(e,
					"Ошибка при применении промокода {Promocode} для заказа {ExternalOrderId} от {Source}",
					applyPromoCodeDto.PromoCode,
					applyPromoCodeDto.ExternalOrderId,
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
