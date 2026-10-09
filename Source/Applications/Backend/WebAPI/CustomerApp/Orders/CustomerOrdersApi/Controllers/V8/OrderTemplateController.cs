using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using CustomerApp.Contracts.Sale.Templates;
using CustomerOrders.Contracts.V8.Sale.Templates;
using CustomerOrdersApi.Library.V8.Dto.Carts;
using CustomerOrdersApi.Library.V8.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.Presentation.WebApi.Messages;

namespace CustomerOrdersApi.Controllers.V8
{
	[ApiVersion("8.0")]
	public class OrderTemplateController : VersionedController
	{
		private readonly CustomerTemplateService _templateService;

		public OrderTemplateController(
			ILogger<ApiControllerBase> logger,
			CustomerTemplateService templateService
			) : base(logger)
		{
			_templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
		}
		
		/// <summary>
		/// Получение условий доставки для автозаказа(шаблона)
		/// </summary>
		/// <param name="request">Данные для предоставления настроек доставки автозаказа <see cref="GetOrderTemplateScheduleOptionsRequest"/></param>
		/// <returns>Результат проверки <see cref="GetOrderTemplateScheduleOptionsResponse"/></returns>
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetOrderTemplateScheduleOptionsResponse))]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
		[HttpGet]
		public IActionResult GetOrderTemplateScheduleOptions(GetOrderTemplateScheduleOptionsRequest request)
		{
			try
			{
				_logger.LogInformation("Поступил запрос получения настроек доставки автозаказа {@TemplateScheduleOptionsRequest}", request);

				var result = _templateService.GetOrderTemplateScheduleOptions(request);

				if(result.IsFailure)
				{
					var error = result.Errors.First();
					return Problem(error.Message, title: "Некорректные параметры запроса", statusCode: int.Parse(error.Code));
				}
				
				return Ok(result.Value);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при получении настроек доставки автозаказа по {OnlineOrderId} от {Source}",
					request.OnlineOrderId,
					request.Source.ToString());
				
				return Problem(ResponseMessage.HasErrorOccurredPleaseTryAgainLater);
			}
		}
		
		/// <summary>
		/// Проверка параметров автозаказа
		/// </summary>
		/// <param name="request">Данные для предоставления настроек автозаказа <see cref="CheckOrderTemplateRequest"/></param>
		/// <returns>Результат проверки <see cref="OrderConditionsResponse"/></returns>
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CheckOrderTemplateResponse))]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[HttpGet]
		public IActionResult CheckOrderTemplate(CheckOrderTemplateRequest request)
		{
			try
			{
				_logger.LogInformation("Поступил запрос проверки параметров автозаказа {@CheckOrderTemplateRequest}", request);

				var result = _templateService.CheckOrderTemplate(request);
				return Ok(result);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при получении настроек автозаказа по {OnlineOrderId} от {Source}",
					request.OnlineOrderId,
					request.Source.ToString());
				
				return Problem(ResponseMessage.HasErrorOccurredPleaseTryAgainLater);
			}
		}
		
		/// <summary>
		/// Создание шаблона автозаказа
		/// </summary>
		/// <param name="request">Данные для создания шаблона <see cref="CreateOrderTemplateRequest"/></param>
		/// <returns>Результат <see cref="OrderConditionsResponse"/></returns>
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateOrderTemplateResponse))]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[HttpGet]
		public IActionResult CreateOrderTemplate(CreateOrderTemplateRequest request)
		{
			try
			{
				_logger.LogInformation("Поступил запрос создания шаблона автозаказа {@CreateOrderTemplateRequest}", request);

				var result = _templateService.CreateOrderTemplate(request);
				return Ok(result);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при получении настроек автозаказа по {OnlineOrderId} от {Source}",
					request.OnlineOrderId,
					request.Source.ToString());
				
				return Problem(ResponseMessage.HasErrorOccurredPleaseTryAgainLater);
			}
		}
	}
}
