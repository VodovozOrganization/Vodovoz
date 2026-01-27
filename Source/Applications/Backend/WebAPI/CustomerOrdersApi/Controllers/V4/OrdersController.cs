using System;
using System.Linq;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.Common;
using CustomerOrdersApi.Library.V4.Dto.Orders;
using CustomerOrdersApi.Library.V4.Services;
using Gamma.Utilities;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Presentation.WebApi.Messages;

namespace CustomerOrdersApi.Controllers.V4
{
	public class OrdersController : SignatureControllerBase
	{
		private readonly ICustomerOrdersService _customerOrdersService;
		private readonly IRequestClient<CreatingOnlineOrder> _requestClient;

		public OrdersController(
			ILogger<OrdersController> logger,
			ICustomerOrdersService customerOrdersService,
			IRequestClient<CreatingOnlineOrder> requestClient
			) : base(logger)
		{
			_customerOrdersService = customerOrdersService ?? throw new ArgumentNullException(nameof(customerOrdersService));
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
				var response = await _requestClient.GetResponse<CreatedOnlineOrder>(creatingOnlineOrder);

				return response.Message.Code switch
				{
					200 => Ok(response.Message.OnlineOrderId),
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
		public IActionResult ChangeOrder(ChangingOrderDto changingOrderDto)
		{
			var sourceName = changingOrderDto.Source.GetEnumTitle();

			try
			{
				_logger.LogInformation("Поступил запрос на изменение заказа {@ChangeOrderRequest}", changingOrderDto);

				var result = _customerOrdersService.UpdateOrder(changingOrderDto);

				if(result.IsSuccess)
				{
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
	}
}
