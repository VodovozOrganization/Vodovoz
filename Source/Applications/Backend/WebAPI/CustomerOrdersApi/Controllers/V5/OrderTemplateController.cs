using System;
using System.Data.Bindings;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Core.Infrastructure;
using CustomerOrders.Contracts.V5.Orders.Templates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Core.Domain.Repositories;
using VodovozBusiness.Services.Orders;

namespace CustomerOrdersApi.Controllers.V5
{
	[ApiVersion("5.0")]
	[Authorize]
	public class OrderTemplateController : VersionedController
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IGenericRepository<OnlineOrderTemplate> _orderTemplateRepository;
		private readonly IOnlineOrderTemplateHandler _onlineOrderTemplateHandler;

		public OrderTemplateController(
			ILogger<OrderTemplateController> logger,
			IUnitOfWorkFactory uowFactory,
			IGenericRepository<OnlineOrderTemplate> orderTemplateRepository,
			IOnlineOrderTemplateHandler onlineOrderTemplateHandler
			) : base(logger)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_orderTemplateRepository = orderTemplateRepository ?? throw new ArgumentNullException(nameof(orderTemplateRepository));
			_onlineOrderTemplateHandler = onlineOrderTemplateHandler ?? throw new ArgumentNullException(nameof(onlineOrderTemplateHandler));
		}

		/// <summary>
		/// Получение информации о шаблоне автозаказа
		/// </summary>
		/// <param name="orderTemplateDto">Данные запроса на получение информации о шаблоне <see cref="GetOrderTemplateInfoDto"/></param>
		/// <returns>
		/// 200 с информацией о шаблоне <see cref="OrderTemplateInfoDto"/>
		/// 404 - не найден шаблон
		/// 500 - ошибка
		/// </returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderTemplateInfoDto))]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> GetOrderTemplateInfo([FromBody] GetOrderTemplateInfoDto orderTemplateDto)
		{
			var sourceName = orderTemplateDto.Source.GetEnumTitle();
			var templateId = orderTemplateDto.OrderTemplateId;
			
			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} на получение шаблона {OrderTemplateId}",
					sourceName,
					templateId);

				using var uow = _uowFactory.CreateWithoutRoot();
				var templateInfo = await _onlineOrderTemplateHandler.GetFreshOnlineOrderTemplateDataAsync(uow, templateId);

				if(templateInfo is null)
				{
					return Problem($"Не найден шаблон с идентификатором {templateId}", statusCode: 404);
				}
				
				return Ok(templateInfo);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при получении деталей шаблона {OrderTemplateId} от {Source}",
					orderTemplateDto.OrderTemplateId,
					sourceName);

				return Problem();
			}
		}

		/// <summary>
		/// Получение шаблонов автозаказов клиента
		/// </summary>
		/// <param name="orderTemplatesDto">Данные запроса получения шаблонов клиента</param>
		/// <returns>
		/// 200 со списком шаблонов клиента <see cref="OrderTemplatesDto"/>
		/// 500 - ошибка
		/// </returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderTemplatesDto))]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public IActionResult GetOrderTemplates([FromBody] GetOrderTemplatesDto orderTemplatesDto)
		{
			var sourceName = "Неизвестный источник";
			
			try
			{
				sourceName = orderTemplatesDto.Source.ToString().TryParseAsEnum<Source>().GetEnumTitle();
			
				_logger.LogInformation(
					"Поступил запрос от {Source} на получение шаблонов автозаказа клиента {CounterpartyId} страница {Page}",
					sourceName,
					orderTemplatesDto.CounterpartyErpId,
					orderTemplatesDto.Page);
				
				using var uow = _uowFactory.CreateWithoutRoot();

				var skipElements = (orderTemplatesDto.Page - 1) * orderTemplatesDto.TemplatesCountOnPage;
				var orderTemplates =
					_onlineOrderTemplateHandler.GetOnlineOrdersTemplatesList(
						uow, orderTemplatesDto.CounterpartyErpId, skipElements, orderTemplatesDto.TemplatesCountOnPage);

				return Ok(orderTemplates);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при получении шаблонов автозаказов клиента {CounterpartyId} от {Source}",
					orderTemplatesDto.CounterpartyErpId,
					sourceName);

				return Problem();
			}
		}

		/// <summary>
		/// Обновление шаблона автозаказа
		/// </summary>
		/// <param name="updateOrderTemplateDto">Информация для обновления шаблона <see cref="UpdateOrderTemplateDto"/></param>
		/// <returns>
		/// 200 - успех
		/// 404 - не найден шаблон
		/// 500 - в случае ошибки
		/// </returns>
		[HttpPatch]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OkResult))]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<IActionResult> UpdateOrderTemplate([FromBody] UpdateOrderTemplateDto updateOrderTemplateDto)
		{
			var sourceName = updateOrderTemplateDto.Source.GetEnumTitle();
			var orderTemplateId = updateOrderTemplateDto.OrderTemplateId;
			
			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} на обновление шаблона {OrderTemplateId}",
					sourceName,
					orderTemplateId);

				using var uow = _uowFactory.CreateWithoutRoot("Обновление шаблона автозаказа из ИПЗ");
				var orderTemplate = _orderTemplateRepository
					.Get(uow, x => x.Id == orderTemplateId)
					.FirstOrDefault();

				if(orderTemplate is null)
				{
					return Problem($"Не найден шаблон с идентификатором {orderTemplateId}", statusCode: 404);
				}

				orderTemplate.UpdateState(updateOrderTemplateDto.IsActive, updateOrderTemplateDto.IsArchive);

				await uow.SaveAsync(orderTemplate);
				await uow.CommitAsync();

				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при обновлении шаблона {OrderTemplateId} от {Source}",
					orderTemplateId,
					sourceName);

				return Problem();
			}
		}
	}
}
