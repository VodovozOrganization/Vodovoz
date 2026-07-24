using System;
using System.Linq;
using CustomerOrdersApi.Library.V7.Dto.Orders.FixedPrice;
using CustomerOrdersApi.Library.V7.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerOrdersApi.Controllers.V7
{
	/// <summary>
	/// Контроллер для работы с фиксой
	/// </summary>
	[ApiVersion("7.0")]
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
		/// 204 - если нет фиксы
		///	200 - если фикса есть. Тело ответа будет содержать список товаров
		/// </returns>
		[HttpGet]
		public IActionResult ApplyFixedPriceToOrder([FromBody] ApplyFixedPriceDto applyFixedPriceDto)
		{
			var sourceName = applyFixedPriceDto.Source.GetEnumTitle();
			
			try
			{
				_logger.LogInformation("Поступил запрос на применение фиксы {@FixedPriceRequest}, проверяем...", applyFixedPriceDto);
				
				if(!_fixedPriceService.ValidateApplyingFixedPriceSignature(applyFixedPriceDto, out var generatedSignature))
				{
					return InvalidSignature(applyFixedPriceDto.Signature, generatedSignature);
				}

				_logger.LogInformation("Подпись валидна, пробуем применить фиксу...");
				var result = _fixedPriceService.ApplyFixedPriceToOnlineOrder(applyFixedPriceDto);

				if(result.IsSuccess)
				{
					_logger.LogInformation("Отправляем ответ по фиксе: {@FixedPriceResponse}", result.Value);
					return Ok(result.Value);
				}

				var failureResult = result.Errors.First().Message;
				_logger.LogWarning("Фикса не добавлена: {FixedPriceFailure}", failureResult);
				return Problem(detail: failureResult, statusCode: 204, title: "Фикса не добавлена");
			}
			catch(Exception e)
			{
				_logger.LogError(e,
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
