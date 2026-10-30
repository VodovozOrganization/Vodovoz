using System;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Core.Infrastructure;
using CustomerApp.Contracts.Sale.Templates;
using CustomerOrders.Contracts.V8.Sale.Templates;
using CustomerOrdersApi.Library.V8.Dto.Carts;
using CustomerOrdersApi.Library.V8.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Presentation.WebApi.Common;
using Vodovoz.Presentation.WebApi.Messages;

namespace CustomerOrdersApi.Controllers.V8
{
	[ApiVersion("8.0")]
	[Authorize]
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

				if(result.IsFailure)
				{
					var error = result.Errors.First();
					return Problem(error.Message, statusCode: int.Parse(error.Code));
				}
				
				return Ok(result.Value);
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
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Результат <see cref="OrderConditionsResponse"/></returns>
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateOrderTemplateResponse))]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
		[HttpGet]
		public async Task<IActionResult> CreateOrderTemplate(
			CreateOrderTemplateRequest request,
			CancellationToken cancellationToken
			)
		{
			try
			{
				_logger.LogInformation("Поступил запрос создания шаблона автозаказа {@CreateOrderTemplateRequest}", request);

				var result = await _templateService.CreateOrderTemplate(request, cancellationToken);

				if(result.IsFailure)
				{
					var error = result.Errors.First();
					return Problem(error.Message, statusCode: int.Parse(error.Code));
				}
				
				return Ok(result.Value);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при создании шаблона автозаказа по {OnlineOrderId} от {Source}",
					request.OnlineOrderId,
					request.Source.ToString());
				
				return Problem(ResponseMessage.HasErrorOccurredPleaseTryAgainLater);
			}
		}

		/// <summary>
		/// Получение информации о шаблоне автозаказа
		/// </summary>
		/// <param name="orderTemplateDto">Данные запроса на получение информации о шаблоне <see cref="GetOrderTemplateInfoDto"/></param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>
		/// 200 с информацией о шаблоне <see cref="OrderTemplateInfoDto"/>
		/// 404 - не найден шаблон
		/// 500 - ошибка
		/// </returns>
		[HttpPost]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderTemplateInfoDto))]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> GetOrderTemplateInfo(
			GetOrderTemplateInfoDto orderTemplateDto,
			CancellationToken cancellationToken)
		{
			var sourceName = orderTemplateDto.Source.ToString();
			var templateId = orderTemplateDto.OrderTemplateId;
			
			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} на получение информации об автозаказе {OrderTemplateId}",
					sourceName,
					templateId);

				var result = await _templateService.GetOrderTemplateInfoAsync(orderTemplateDto, cancellationToken);

				if(result.IsFailure)
				{
					var error = result.Errors.First();
					return Problem(error.Message, statusCode: int.Parse(error.Code));
				}

				return Ok(result);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при получении деталей шаблона {OrderTemplateId} от {Source}",
					orderTemplateDto.OrderTemplateId,
					sourceName);

				return Problem(ResponseMessage.HasErrorOccurredPleaseTryAgainLater);
			}
		}

		/// <summary>
		/// Получение шаблонов автозаказов клиента
		/// </summary>
		/// <param name="orderTemplatesDto">Данные запроса получения шаблонов клиента</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>
		/// 200 со списком шаблонов клиента <see cref="OrderTemplatesDto"/>
		/// 500 - ошибка
		/// </returns>
		[HttpPost]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderTemplatesDto))]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public IActionResult GetOrderTemplates(
			GetOrderTemplatesDto orderTemplatesDto,
			CancellationToken cancellationToken)
		{
			var sourceName = orderTemplatesDto.Source.GetEnumDisplayName();
			
			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} на получение шаблонов автозаказа клиента {CounterpartyId} страница {Page}",
					sourceName,
					orderTemplatesDto.ErpCounterpartyId,
					orderTemplatesDto.Page);
				
				var orderTemplates = _templateService.GetOrderTemplatesAsync(orderTemplatesDto, cancellationToken);

				return Ok(orderTemplates);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при получении шаблонов автозаказов клиента {CounterpartyId} от {Source}",
					orderTemplatesDto.ErpCounterpartyId,
					sourceName);

				return Problem(ResponseMessage.HasErrorOccurredPleaseTryAgainLater);
			}
		}
	}
}
