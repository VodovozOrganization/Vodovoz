using CustomerOrdersApi.Library.Common;
using CustomerOrdersApi.Library.V5.Dto.Orders;
using CustomerOrdersApi.Library.V5.Services;
using Gamma.Utilities;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Presentation.WebApi.Messages;
using ChangingOrderDtoV5 = CustomerOrdersApi.Library.V5.Dto.Orders.ChangingOrderDto;

namespace CustomerOrdersApi.Controllers.V5
{
	[ApiVersion("5.0")]
	public class OrdersController : SignatureControllerBase
	{
		private readonly ICustomerOrdersServiceV5 _customerOrdersService;
		private readonly ICustomerOrderCancellationService _orderCancellationService;
		private readonly IRequestClient<CreatingOnlineOrder> _requestClient;

		public OrdersController(
			ILogger<OrdersController> logger,
			ICustomerOrdersServiceV5 customerOrdersService,
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

		[HttpGet]
		public IActionResult GetOrderInfo([FromBody] GetDetailedOrderInfoDto getDetailedOrderInfoDto)
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
				var orderInfo = _customerOrdersService.GetDetailedOrderInfo(getDetailedOrderInfoDto);
				
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
		public async Task<IActionResult> ChangeOrder(ChangingOrderDtoV5 changingOrderDto, CancellationToken cancellationToken)
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
