using CustomerOrdersApi.Library.Common;
using CustomerOrdersApi.Library.V6.Dto.Orders;
using CustomerOrdersApi.Library.V6.Services;
using Gamma.Utilities;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Errors.Orders;
using Vodovoz.Presentation.WebApi.Messages;

namespace CustomerOrdersApi.Controllers.V6
{
	[ApiVersion("6.0")]
	public class OrdersController : SignatureControllerBase
	{
		private readonly ICustomerOrdersServiceV6 _customerOrdersService;
		private readonly ICustomerOrderCancellationService _orderCancellationService;
		private readonly IRequestClient<CreatingOnlineOrder> _requestClient;

		public OrdersController(
			ILogger<OrdersController> logger,
			ICustomerOrdersServiceV6 customerOrdersService,
			ICustomerOrderCancellationService orderCancellationService,
			IRequestClient<CreatingOnlineOrder> requestClient
			) : base(logger)
		{
			_customerOrdersService = customerOrdersService ?? throw new ArgumentNullException(nameof(customerOrdersService));
			_orderCancellationService = orderCancellationService ?? throw new ArgumentNullException(nameof(orderCancellationService));
			_requestClient = requestClient ?? throw new ArgumentNullException(nameof(requestClient));
		}

		[HttpPost]
		public async Task<IActionResult> CreateOrderAsync(CreatingOnlineOrder creatingOnlineOrder)
		{
			var sourceName = creatingOnlineOrder.Source.GetEnumTitle();
			
			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} на регистрацию заказа {ExternalOrderId} c подписью {Signature}, проверяем...",
					sourceName,
					creatingOnlineOrder.ExternalOrderId,
					creatingOnlineOrder.Signature);
				
				if(!_customerOrdersService.ValidateOrderSignature(creatingOnlineOrder, out var generatedSignature))
				{
					return InvalidSignature(creatingOnlineOrder.Signature, generatedSignature);
				}

				_logger.LogInformation("Подпись валидна, отправляем в очередь");
				var response = await _requestClient.GetResponse<CreatedOnlineOrderResult>(creatingOnlineOrder);

				return response.Message.Code switch
				{
					200 => Ok(CreatedOnlineOrder.Create(response.Message)),
					409 => Problem(Messages.DuplicatOrderMessage(creatingOnlineOrder.ExternalOrderId), statusCode: response.Message.Code),
					500 => Problem(Messages.ErrorMessage)
				};
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при регистрации заказа {ExternalOrderId} от {Source}",
					creatingOnlineOrder.ExternalOrderId,
					sourceName);

				return Problem(Messages.ErrorMessage);
			}
		}

		/// <summary>
		/// Получение детальной информации о заказе
		/// </summary>
		/// <param name="getDetailedOrderInfoDto">Данные для получения деталей заказа</param>
		/// <param name="cancellationToken">Токен для отмены операции</param>
		/// <returns>Детальная информация о заказе <see cref="DetailedOrderInfoDto"/></returns>
		[HttpGet]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DetailedOrderInfoDto))]
		public async Task<IActionResult> GetOrderInfo(
			[FromBody] GetDetailedOrderInfoDto getDetailedOrderInfoDto,
			CancellationToken cancellationToken
			)
		{
			var sourceName = getDetailedOrderInfoDto.Source.GetEnumTitle();
			var orderId = getDetailedOrderInfoDto.OrderId ?? getDetailedOrderInfoDto.OnlineOrderId;
			
			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} на получение деталей заказа {OrderId} c подписью {Signature}, проверяем...",
					sourceName,
					orderId,
					getDetailedOrderInfoDto.Signature);
				
				if(!_customerOrdersService.ValidateOrderInfoSignature(getDetailedOrderInfoDto, out var generatedSignature))
				{
					return InvalidSignature(getDetailedOrderInfoDto.Signature, generatedSignature);
				}

				_logger.LogInformation("Подпись валидна, получаем данные");
				var orderInfo = await _customerOrdersService.GetDetailedOrderInfo(getDetailedOrderInfoDto, cancellationToken);
				
				return Ok(orderInfo);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при получении деталей заказа {OrderId} от {Source}",
					orderId,
					sourceName);

				return Problem();
			}
		}
		
		[HttpGet]
		public async Task<IActionResult> GetOrders([FromBody] GetOrdersDto getOrdersDto, CancellationToken cancellationToken)
		{
			var sourceName = getOrdersDto.Source.GetEnumTitle();
			
			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} на получение заказов клиента {CounterpartyId} c подписью {Signature}, проверяем...",
					sourceName,
					getOrdersDto.CounterpartyErpId,
					getOrdersDto.Signature);
				
				if(!_customerOrdersService.ValidateCounterpartyOrdersSignature(getOrdersDto, out var generatedSignature))
				{
					return InvalidSignature(getOrdersDto.Signature, generatedSignature);
				}

				_logger.LogInformation(
					"Подпись валидна, получаем заказы клиента {CounterpartyId} страница {Page}",
					getOrdersDto.CounterpartyErpId,
					getOrdersDto.Page);
				
				var orders = await _customerOrdersService.GetOrders(getOrdersDto, cancellationToken);
				
				return Ok(orders);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при получении заказов клиента {CounterpartyId} от {Source}",
					getOrdersDto.CounterpartyErpId,
					sourceName);

				return Problem();
			}
		}

		/// <summary>
		/// Получение текущих активных заказов клиента
		/// </summary>
		/// <param name="getCounterpartyOrdersDto">Данные для получения заказов клиента</param>
		/// <param name="cancellationToken">Токен для отмены операции</param>
		/// <returns>Активные заказы клиента <see cref="ActiveOrdersDto"/> со статусами <c>OrderPerformed</c> и <c>OrderDelivering</c></returns>
		[HttpGet]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ActiveOrdersDto))]
		[Authorize]
		public async Task<IActionResult> GetCurrentClientOrders(
			[FromBody] GetCounterpartyOrdersDto getCounterpartyOrdersDto,
			CancellationToken cancellationToken)
		{
			var sourceName = getCounterpartyOrdersDto.Source.GetEnumTitle();

			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} на получение текущих активных заказов клиента {CounterpartyId}",
					sourceName,
					getCounterpartyOrdersDto.CounterpartyErpId);

				var orders = await _customerOrdersService.GetCurrentClientOrders(getCounterpartyOrdersDto, cancellationToken);

				return Ok(orders);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при получении текущих заказов клиента {CounterpartyId} от {Source}",
					getCounterpartyOrdersDto.CounterpartyErpId,
					sourceName);

				return Problem();
			}
		}

		[HttpGet]
		public IActionResult GetAvailablePaymentMethods([FromBody] GetAvailablePaymentMethodsDto getAvailablePaymentMethods)
		{
			var sourceName = getAvailablePaymentMethods.Source.GetEnumTitle();
			
			try
			{
				_logger.LogInformation(
					"Поступил запрос на получение доступных типов оплат {@AvailablePaymentMethods}", getAvailablePaymentMethods);
				
				var result = _customerOrdersService.GetAvailablePaymentMethods(getAvailablePaymentMethods);

				return result.HttpCode switch
				{
					400 or 408 => Problem(result.Message, statusCode: result.HttpCode),
					500 => Problem(ResponseMessage.HasErrorOccurredPleaseTryAgainLater, statusCode: result.HttpCode),
					_ => Ok(result.AvailablePayments)
				};
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при получении доступных типов оплат {ExternalCounterpartyId} от {Source}",
					getAvailablePaymentMethods.ExternalCounterpartyId,
					sourceName);

				return Problem(ResponseMessage.HasErrorOccurredPleaseTryAgainLater);
			}
		}
		
		[HttpPost]
		public async Task<IActionResult> ChangeOrder(ChangingOrderDto changingOrderDto, CancellationToken cancellationToken)
		{
			var sourceName = changingOrderDto.Source.GetEnumTitle();

			try
			{
				_logger.LogInformation("Поступил запрос на изменение заказа {@ChangeOrderRequest}", changingOrderDto);

				var result = await _customerOrdersService.UpdateOrderAsync(changingOrderDto, cancellationToken);

				if(result.IsSuccess)
				{
					return Ok(result.Value);
				}

				var firstError = result.Errors.FirstOrDefault();
				var statusCode = GetStatusCodeFromError(firstError);

				return Problem(firstError.Message, statusCode: statusCode);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при попытке обновить заказ {OnlineOrderId} от {Source}",
					changingOrderDto.OnlineOrderId,
					sourceName);
				
				return Problem(ResponseMessage.HasErrorOccurredPleaseTryAgainLater);
			}
		}

		/// <summary>
		/// Получение координат курьера и точки доставки
		/// </summary>
		/// <param name="getCourierCoordinatesDto">Данные для получения координат</param>
		/// <param name="cancellationToken">Токен для отмены операции</param>
		/// <returns>Координаты курьера <see cref="CourierCoordinatesDto"/></returns>
		[HttpGet]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CourierCoordinatesDto))]
		[Authorize]
		public async Task<IActionResult> GetCourierCoordinates(
			[FromBody] GetCourierCoordinatesDto getCourierCoordinatesDto,
			CancellationToken cancellationToken)
		{
			var sourceName = getCourierCoordinatesDto.Source.GetEnumTitle();

			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} на получение координат курьера. " +
					"Клиент: {CounterpartyId} " +
					"Идентификатор клиента в ИПЗ: {ExternalCounterpartyId} " +
					"Номер заказа: {OrderId} " +
					"Номер онлайн заказа: {OnlineOrderId}",
					sourceName,
					getCourierCoordinatesDto.CounterpartyErpId,
					getCourierCoordinatesDto.ExternalCounterpartyId,
					getCourierCoordinatesDto.OrderId,
					getCourierCoordinatesDto.OnlineOrderId);

				var courierCoordinatesResult = await _customerOrdersService.GetCourierCoordinates(getCourierCoordinatesDto, cancellationToken);

				if(courierCoordinatesResult.IsFailure)
				{
					var firstError = courierCoordinatesResult.Errors.First();
					return Problem(firstError.Message, statusCode: GetStatusCode(courierCoordinatesResult));
				}

				return Ok(courierCoordinatesResult);
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при получении координат курьера по запросу клиента {CounterpartyId} от {Source}",
					getCourierCoordinatesDto.CounterpartyErpId,
					sourceName);

				return Problem();
			}
		}

		private static int GetStatusCode(Result result)
		{
			if(result.IsSuccess)
			{
				return StatusCodes.Status200OK;
			}

			var firstError = result.Errors.FirstOrDefault();

			if(firstError != null
				&& (firstError.Code == OrderErrors.NotFound
					|| firstError.Code == OnlineOrderErrors.OnlineOrderNotFound
					|| firstError.Code == OnlineOrderErrors.ErpOrderForOnlineOrderNotFound))
			{
				return StatusCodes.Status404NotFound;
			}

			return StatusCodes.Status400BadRequest;
		}

		[HttpPost]
		public async Task<IActionResult> CancelOrderAsync(CancelOrderDto cancelOrderDto, CancellationToken cancellationToken)
		{
			var sourceName = cancelOrderDto.Source.GetEnumTitle();

			if(!cancelOrderDto.OrderId.HasValue && !cancelOrderDto.OnlineOrderId.HasValue)
			{
				_logger.LogWarning(
					"Поступил запрос от {Source} на отмену заказа без указания OrderId или OnlineOrderId",
					sourceName);

				return BadRequest("Необходимо указать OrderId или OnlineOrderId");
			}

			var orderId = cancelOrderDto.OrderId ?? cancelOrderDto.OnlineOrderId;

			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} на отмену заказа {OrderId} от клиента {ClientId}",
					sourceName,
					orderId,
					cancelOrderDto.ErpCounterpartyId);

				var result = await _orderCancellationService.ApplyCancellationAsync(
					cancelOrderDto.Source,
					cancelOrderDto.ErpCounterpartyId,
					cancelOrderDto.OrderId,
					cancelOrderDto.OnlineOrderId,
					cancellationToken);

				if(result.IsSuccess)
				{
					_logger.LogInformation(
						"Заказ {OrderId} успешно отменен", orderId);

					return Ok(result.Value);
				}

				var error = result.Errors.FirstOrDefault();
				var statusCode = GetStatusCodeFromError(error);

				_logger.LogWarning(
					"Отмена заказа {OrderId} не выполнена: {ErrorMessage}",
					orderId,
					error.Message);

				return Problem(
					detail: error.Message,
					statusCode: statusCode,
					title: "One or more validation errors occurred");
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при отмене заказа {OrderId} от {Source}",
					orderId,
					sourceName);

				return Problem("Произошла ошибка, пожалуйста, попробуйте позже");
			}
		}

		private static int GetStatusCodeFromError(Error error)
		{
			if(error is null || string.IsNullOrEmpty(error.Code))
			{
				return 400;
			}

			return int.TryParse(error.Code, out var code) ? code : 400;
		}
	}
}
