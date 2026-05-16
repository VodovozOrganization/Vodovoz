using CustomerOrders.Contracts.V5.Carts;
using CustomerOrders.Contracts.V5.Orders;
using CustomerOrders.Contracts.V5.Orders.PromoCodes;
using CustomerOrdersApi.Library.V5.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerOrdersApi.Controllers.V5
{
	[ApiVersion("5.0")]
	public class DiscountController : SignatureControllerBase
	{
		private readonly ICustomerOrdersDiscountServiceV5 _discountService;

		public DiscountController(
			ILogger<SignatureControllerBase> logger,
			ICustomerOrdersDiscountServiceV5 discountService
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

		/// <summary>
		/// Проверка доступности использования скидки на первый заказ для клиента
		/// </summary>
		/// <param name="request">Данные клиента и источника запроса <see cref="FirstOrderDiscountConditionsRequestDto"/></param>
		/// <returns>Результат проверки <see cref="FirstOrderDiscountConditionsDto"/></returns>
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FirstOrderDiscountConditionsDto))]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetFirstOrderDiscountConditions([FromBody] FirstOrderDiscountConditionsRequestDto requestDto, CancellationToken cancellationToken)
		{
			var sourceName = requestDto.Source.GetEnumTitle();

			try
			{
				_logger.LogInformation("Поступил запрос доступности использования скидки на первый заказ для клиента {@FirstOrderDiscountConditionsRequest}, проверяем...", requestDto);

				var result =
					_discountService.GetFirstOrderDiscountConditions(
						requestDto.Source,
						requestDto.ExternalCounterpartyId,
						requestDto.СounterpartyErpId,
						cancellationToken); ;

				return Ok(result);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при проверке доступности использования скидки на первый заказ для клиента " +
					"ExternalCounterpartyId = {ExternalClientId}, СounterpartyErpId = {СounterpartyErpId} от {Source}",
					requestDto.ExternalCounterpartyId,
					requestDto.СounterpartyErpId,
					requestDto.ExternalCounterpartyId,
					sourceName);

				return Problem();
			}
		}
	}
}
