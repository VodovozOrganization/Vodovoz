using System;
using System.Linq;
using CustomerOrdersApi.Library.Default.Dto.Orders.FixedPrice;
using CustomerOrdersApi.Library.Default.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerOrdersApi.Controllers.Default
{
	/// <summary>
	/// Контроллер для работы с фиксой
	/// </summary>
	public class FixedPriceController : SignatureControllerBase
	{
		private readonly ICustomerOrderFixedPriceService _fixedPriceService;

		public FixedPriceController(
			ILogger<SignatureControllerBase> logger,
			ICustomerOrderFixedPriceService fixedPriceService) : base(logger)
		{
			_fixedPriceService = fixedPriceService ?? throw new ArgumentNullException(nameof(fixedPriceService));
		}
		
		/// <summary>
		/// Эндпойнт применения фиксы
		/// </summary>
		/// <param name="applyFixedPriceDto">Информация по заказу для применения фиксы</param>
		/// <returns>
		///	500 - в случае ошибки
		/// 404 - если нет фиксы
		///	200 - если фикса есть. Тело ответа будет содержать список товаров <see cref="AppliedFixedPriceDto"/>
		/// </returns>
		[HttpGet]
		public IActionResult ApplyFixedPriceToOrder([FromBody] ApplyFixedPriceDto applyFixedPriceDto)
		{
			var sourceName = applyFixedPriceDto.Source.GetEnumTitle();
			
			try
			{
				Logger.LogInformation("Поступил запрос на применение фиксы {@FixedPriceRequest}, проверяем...", applyFixedPriceDto);
				
				if(!_fixedPriceService.ValidateApplyingFixedPriceSignature(applyFixedPriceDto, out var generatedSignature))
				{
					return InvalidSignature(applyFixedPriceDto.Signature, generatedSignature);
				}

				Logger.LogInformation("Подпись валидна, пробуем применить фиксу...");
				var result = _fixedPriceService.ApplyFixedPriceToOnlineOrder(applyFixedPriceDto);

				if(result.IsSuccess)
				{
					Logger.LogInformation("Отправляем ответ по фиксе: {@FixedPriceResponse}", result.Value);
					return Ok(
						new AppliedFixedPriceDto
						{
							OnlineOrderItems = result.Value
						});
				}

				return NotFound(result.Errors.First().Message);
			}
			catch(Exception e)
			{
				Logger.LogError(e,
					"Ошибка при применении фиксы для заказа {ExternalOrderId}" +
					" пользователя {ExternalClientId} от {Source}",
					applyFixedPriceDto.ExternalOrderId,
					applyFixedPriceDto.ExternalCounterpartyId,
					sourceName);

				return Problem();
			}
		}
	}
}
