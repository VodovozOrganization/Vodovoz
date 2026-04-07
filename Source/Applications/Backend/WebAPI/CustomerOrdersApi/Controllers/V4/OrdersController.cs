using CustomerNotifications.Contracts.Messages;
using CustomerNotifications.Publisher.Services;
using CustomerOrdersApi.Library.Common;
using CustomerOrdersApi.Library.V4.Dto.Orders;
using CustomerOrdersApi.Library.V4.Services;
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
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Errors.Logistics;
using Vodovoz.Errors.Orders;
using Vodovoz.Errors.TrueMark;
using Vodovoz.Presentation.WebApi.Messages;

namespace CustomerOrdersApi.Controllers.V4
{
	[ApiVersion("4.0")]
	public class OrdersController : SignatureControllerBase
	{
		private readonly ICustomerOrdersServiceV4 _customerOrdersService;
		private readonly IRequestClient<CreatingOnlineOrder> _requestClient;
		private readonly ICustomerNotificationPublisher _customerNotificationPublisher;

		public OrdersController(
			ILogger<OrdersController> logger,
			ICustomerOrdersServiceV4 customerOrdersService,
			IRequestClient<CreatingOnlineOrder> requestClient,
			ICustomerNotificationPublisher customerNotificationPublisher
			) : base(logger)
		{
			_customerOrdersService = customerOrdersService ?? throw new ArgumentNullException(nameof(customerOrdersService));
			_requestClient = requestClient ?? throw new ArgumentNullException(nameof(requestClient));
			_customerNotificationPublisher = customerNotificationPublisher ?? throw new ArgumentNullException(nameof(customerNotificationPublisher));
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
		public async Task<IActionResult> GetOrderInfo([FromBody] GetDetailedOrderInfoDto getDetailedOrderInfoDto, CancellationToken cancellationToken)
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
		public IActionResult GetOrders([FromBody] GetOrdersDto getOrdersDto)
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
				
				var orders = _customerOrdersService.GetOrders(getOrdersDto);
				
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
					var sourcesForPaymentAwaitingNotification = new[] { OnlinePaymentSource.FromMobileApp, OnlinePaymentSource.FromMobileAppByQr, OnlinePaymentSource.FromMobileAppByYandexSplit };

					var needPaymentAwaitingNotification =
						changingOrderDto.PaymentStatus == OnlineOrderPaymentStatus.UnPaid
						&& changingOrderDto.OnlinePaymentSource != null
						&& sourcesForPaymentAwaitingNotification.Contains(changingOrderDto.OnlinePaymentSource.Value)
						&& changingOrderDto.OnlineOrderId != null;

					if(needPaymentAwaitingNotification)
					{
						await _customerNotificationPublisher.PublishAsync(
							new CustomerNotificationMessage
							{
								CustomerNotificationEventType = CustomerNotificationEventType.OrderAwaitingPayment,
								OnlineOrderId = changingOrderDto.OnlineOrderId.Value
							});
					}

					var needOrderPaidNotification =
						changingOrderDto.PaymentStatus == OnlineOrderPaymentStatus.Paid
						&& changingOrderDto.OnlinePayment != null
						&& changingOrderDto.OnlineOrderPaymentType != null
						&& changingOrderDto.OnlineOrderId != null;

					if(needOrderPaidNotification)
					{
						await _customerNotificationPublisher.PublishAsync(
							new CustomerNotificationMessage
							{
								CustomerNotificationEventType = CustomerNotificationEventType.OrderPaid,
								OnlineOrderId = changingOrderDto.OnlineOrderId.Value
							});
					}



					return Ok(result.Value);
				}

				var firstError = result.Errors.First();
				return Problem(firstError.Message, statusCode: int.Parse(firstError.Code));
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
	}
}
