using System;
using System.Data.Bindings;
using System.Linq;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.V5.Dto.Orders.OrderTemplates;
using CustomerOrdersApi.Library.V5.Factories;
using CustomerOrdersApi.Library.V5.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Core.Data.Orders.V5;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Orders;
using Vodovoz.Settings.Orders;

namespace CustomerOrdersApi.Controllers.V5
{
	public class OrderTemplateController : VersionedController
	{
		private readonly IDiscountReasonSettings _discountReasonSettings;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly CustomerAppOrderTemplateRepository _orderTemplateDtoRepository;
		private readonly IGenericRepository<OnlineOrderTemplate> _orderTemplateRepository;
		private readonly OnlineOrderTemplateHandler _onlineOrderTemplateHandler;
		private readonly IInfoMessageFactoryV5 _infoMessageFactoryV5;

		public OrderTemplateController(
			ILogger<OrderTemplateController> logger,
			IDiscountReasonSettings discountReasonSettings,
			IUnitOfWorkFactory uowFactory,
			CustomerAppOrderTemplateRepository orderTemplateDtoRepository,
			IGenericRepository<OnlineOrderTemplate> orderTemplateRepository,
			OnlineOrderTemplateHandler onlineOrderTemplateHandler,
			IInfoMessageFactoryV5 infoMessageFactoryV5
			) : base(logger)
		{
			_discountReasonSettings = discountReasonSettings ?? throw new ArgumentNullException(nameof(discountReasonSettings));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_orderTemplateDtoRepository = orderTemplateDtoRepository ?? throw new ArgumentNullException(nameof(orderTemplateDtoRepository));
			_orderTemplateRepository = orderTemplateRepository ?? throw new ArgumentNullException(nameof(orderTemplateRepository));
			_onlineOrderTemplateHandler = onlineOrderTemplateHandler ?? throw new ArgumentNullException(nameof(onlineOrderTemplateHandler));
			_infoMessageFactoryV5 = infoMessageFactoryV5 ?? throw new ArgumentNullException(nameof(infoMessageFactoryV5));
		}

		/// <summary>
		/// Получение информации о шаблоне автозаказа
		/// </summary>
		/// <param name="orderTemplateDto">Данные запроса на получение информации о шаблоне <see cref="GetOrderTemplateInfoDto"/></param>
		/// <returns>
		/// 200 с информацией о шаблоне <see cref="OrderTemplateInfoDto"/>
		/// 500 - ошибка
		/// </returns>
		[HttpGet]
		public IActionResult GetOrderTemplateInfo([FromBody] GetOrderTemplateInfoDto orderTemplateDto)
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
				var templateInfo = _onlineOrderTemplateHandler.GetFreshOnlineOrderTemplateData(uow, templateId);

				if(templateInfo is null)
				{
					return Problem("Не найден шаблон с таким идентификатором", statusCode: 404);
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
		public IActionResult GetOrderTemplates([FromBody] GetOrderTemplatesDto orderTemplatesDto)
		{
			var sourceName = orderTemplatesDto.Source.GetEnumTitle();
			
			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} на получение шаблонов автозаказа клиента {CounterpartyId} страница {Page}",
					sourceName,
					orderTemplatesDto.CounterpartyErpId,
					orderTemplatesDto.Page);
				
				using var uow = _uowFactory.CreateWithoutRoot();
				var orderTemplates =
					_orderTemplateDtoRepository.GetOrderTemplates(uow, orderTemplatesDto.CounterpartyErpId);
				var skipElements = (orderTemplatesDto.Page - 1) * orderTemplatesDto.TemplatesCountOnPage;

				var templates = orderTemplates
					.Skip(skipElements)
					.Take(orderTemplatesDto.TemplatesCountOnPage)
					.ToArray();
				
				return Ok(OrderTemplatesDto.Create(templates));
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
		/// 500 - в случае ошибки
		/// </returns>
		[HttpPatch]
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
		
		/// <summary>
		/// Получение информации о скидке при автозаказе
		/// </summary>
		/// <param name="templateDiscountInfoDto">Данные запроса получения инфы о скидке при автозаказе</param>
		/// <returns>
		/// 200 с сообщением о скидке<see cref="OrderTemplatesDto"/>
		/// 500 - ошибка
		/// </returns>
		[HttpGet]
		public IActionResult GetOrderTemplateDiscountMessage([FromBody] GetTemplateDiscountInfoDto templateDiscountInfoDto)
		{
			var sourceName = templateDiscountInfoDto.Source.GetEnumTitle();
			
			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} и клиента {CounterpartyId} на получение сообщения о скидке для автозаказа",
					sourceName,
					templateDiscountInfoDto.CounterpartyErpId);
				
				using var uow = _uowFactory.CreateWithoutRoot("Получение информации о скидке при автозаказе");
				var orderTemplateDiscount = uow.GetById<DiscountReason>(_discountReasonSettings.GetAutoOrderDiscountReasonId);

				if(orderTemplateDiscount is null)
				{
					return Problem("Не найдена скидка на автозаказ");
				}
				
				return Ok(_infoMessageFactoryV5.CreateAutoOrderDiscountInfoMessage(orderTemplateDiscount.Value, orderTemplateDiscount.ValueType));
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при получении сообщения о скидке для автозаказа клиента {CounterpartyId} от {Source}",
					templateDiscountInfoDto.CounterpartyErpId,
					sourceName);

				return Problem();
			}
		}
	}
}
