using CustomerOrdersApi.Library.Dto.Orders;
using CustomerOrdersApi.Library.Services;
using Gamma.Utilities;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Vodovoz.Application.Orders.Services.OrderCancellation;
using VodovozHealthCheck.Helpers;

namespace CustomerOrdersApi.Controllers
{
	public class OrdersController : SignatureControllerBase
	{
		private readonly ICustomerOrdersService _customerOrdersService;
		private readonly IOrderTransferService _orderTransferService;
		private readonly IOrderCancellationService _orderCancellationService;
		private readonly IPublishEndpoint _publishEndpoint;

		public OrdersController(
			ILogger<OrdersController> logger,
			ICustomerOrdersService customerOrdersService,
			IOrderTransferService orderTransferService,
			IOrderCancellationService orderCancellationService,
			IPublishEndpoint publishEndpoint
			) : base(logger)
		{
			_customerOrdersService = customerOrdersService ?? throw new ArgumentNullException(nameof(customerOrdersService));
			_orderTransferService = orderTransferService ?? throw new ArgumentNullException(nameof(orderTransferService));
			_orderCancellationService = orderCancellationService ?? throw new ArgumentNullException(nameof(orderCancellationService));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		[HttpPost]
		public async Task<IActionResult> CreateOrderAsync(OnlineOrderInfoDto onlineOrderInfoDto)
		{
			var sourceName = onlineOrderInfoDto.Source.GetEnumTitle();
			
			try
			{
				Logger.LogInformation(
					"Поступил запрос от {Source} на регистрацию заказа {ExternalOrderId} от пользователя {ExternalCounterpartyId}" +
					" клиента {ClientId} с контактным номером {ContactPhone} c подписью {Signature}, проверяем...",
					sourceName,
					onlineOrderInfoDto.ExternalOrderId,
					onlineOrderInfoDto.ExternalCounterpartyId,
					onlineOrderInfoDto.CounterpartyErpId,
					onlineOrderInfoDto.ContactPhone,
					onlineOrderInfoDto.Signature);
				
				if(!_customerOrdersService.ValidateOrderSignature(onlineOrderInfoDto, out var generatedSignature))
				{
					return InvalidSignature(onlineOrderInfoDto.Signature, generatedSignature);
				}

				Logger.LogInformation("Подпись валидна, отправляем в очередь");

				var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);
				
				if(!isDryRun)
				{
					await _publishEndpoint.Publish(onlineOrderInfoDto);
				}

				return Accepted();
			}
			catch(Exception e)
			{
				Logger.LogError(e,
					"Ошибка при регистрации заказа {ExternalOrderId} от пользователя {ExternalCounterpartyId} клиента {ClientId} с контактным номером {ContactPhone} от {Source}",
					onlineOrderInfoDto.ExternalOrderId,
					onlineOrderInfoDto.ExternalCounterpartyId,
					onlineOrderInfoDto.CounterpartyErpId,
					onlineOrderInfoDto.ContactPhone,
					sourceName);

				return Problem();
			}
		}

		[HttpGet]
		public IActionResult GetOrderInfo([FromBody] GetDetailedOrderInfoDto getDetailedOrderInfoDto)
		{
			var sourceName = getDetailedOrderInfoDto.Source.GetEnumTitle();
			var orderId = getDetailedOrderInfoDto.OrderId ?? getDetailedOrderInfoDto.OnlineOrderId;
			
			try
			{
				Logger.LogInformation(
					"Поступил запрос от {Source} на получение деталей заказа {OrderId} c подписью {Signature}, проверяем...",
					sourceName,
					orderId,
					getDetailedOrderInfoDto.Signature);
				
				if(!_customerOrdersService.ValidateOrderInfoSignature(getDetailedOrderInfoDto, out var generatedSignature))
				{
					return InvalidSignature(getDetailedOrderInfoDto.Signature, generatedSignature);
				}

				Logger.LogInformation("Подпись валидна, получаем данные");
				var orderInfo = _customerOrdersService.GetDetailedOrderInfo(getDetailedOrderInfoDto);
				
				return Ok(orderInfo);
			}
			catch(Exception e)
			{
				Logger.LogError(e,
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
				Logger.LogInformation(
					"Поступил запрос от {Source} на получение заказов клиента {CounterpartyId} c подписью {Signature}, проверяем...",
					sourceName,
					getOrdersDto.CounterpartyErpId,
					getOrdersDto.Signature);
				
				if(!_customerOrdersService.ValidateCounterpartyOrdersSignature(getOrdersDto, out var generatedSignature))
				{
					return InvalidSignature(getOrdersDto.Signature, generatedSignature);
				}

				Logger.LogInformation(
					"Подпись валидна, получаем заказы клиента {CounterpartyId} страница {Page}",
					getOrdersDto.CounterpartyErpId,
					getOrdersDto.Page);
				
				var orders = _customerOrdersService.GetOrders(getOrdersDto);
				
				return Ok(orders);
			}
			catch(Exception e)
			{
				Logger.LogError(e,
					"Ошибка при получении заказов клиента {CounterpartyId} от {Source}",
					getOrdersDto.CounterpartyErpId,
					sourceName);

				return Problem();
			}
		}

		[HttpPost]
		public async Task<IActionResult> TransferOrderAsync(TransferOrderDto transferOrderDto)
		{
			var sourceName = transferOrderDto.Source.GetEnumTitle();

			try
			{
				Logger.LogInformation(
					"Поступил запрос от {Source} на перенос заказа {ExternalOrderId} на дату {DeliveryDate} с интервалом {DeliveryScheduleId} c подписью {Signature}, проверяем...",
					sourceName,
					transferOrderDto.ExternalOrderId,
					transferOrderDto.DeliveryDate,
					transferOrderDto.DeliveryScheduleId,
					transferOrderDto.Signature);

				if(!_customerOrdersService.ValidateTransferOrderSignature(transferOrderDto, out var generatedSignature))
				{
					return InvalidSignature(transferOrderDto.Signature, generatedSignature);
				}

				Logger.LogInformation("Подпись валидна, выполняем перенос заказа");

				var transferResult = await _orderTransferService.TransferOrderAsync(transferOrderDto);

				Logger.LogInformation(
					"Результат переноса: IsSuccess={IsSuccess}, StatusCode={StatusCode}",
					transferResult.IsSuccess,
					transferResult.StatusCode);

				return StatusCode(transferResult.StatusCode, new
				{
					title = transferResult.Title,
					status = transferResult.StatusCode,
					detail = transferResult.DetailMessage
				});
			}
			catch(Exception e)
			{
				Logger.LogError(e,
					"Ошибка при переносе заказа {ExternalOrderId} от {Source}",
					transferOrderDto.ExternalOrderId,
					sourceName);

				return StatusCode(500, new
				{
					title = "One or more validation errors occurred",
					status = 500,
					detail = "Произошла ошибка, пожалуйста, попробуйте позже"
				});
			}
		}

		[HttpPost]
		public async Task<IActionResult> CancelOrderAsync(CancelOrderDto cancelOrderDto)
		{
			var sourceName = cancelOrderDto.Source.GetEnumTitle();

			try
			{
				Logger.LogInformation(
					"Поступил запрос от {Source} на отмену заказа {ExternalOrderId} c подписью {Signature}, проверяем...",
					sourceName,
					cancelOrderDto.ExternalOrderId,
					cancelOrderDto.Signature);

				if(!_customerOrdersService.ValidateCancelOrderSignature(cancelOrderDto, out var generatedSignature))
				{
					return InvalidSignature(cancelOrderDto.Signature, generatedSignature);
				}

				Logger.LogInformation("Подпись валидна, проверяем возможность отмены");

				var cancellationResult = await _orderCancellationService.CancelOrderAsync(cancelOrderDto);

				Logger.LogInformation(
					"Результат отмены: Success={Success}, StatusCode={StatusCode}",
					cancellationResult.Success,
					cancellationResult.StatusCode);

				return StatusCode(cancellationResult.StatusCode, new
				{
					title = cancellationResult.Title,
					status = cancellationResult.StatusCode,
					detail = cancellationResult.Detail
				});
			}
			catch(Exception e)
			{
				Logger.LogError(e,
					"Ошибка при отмене заказа {ExternalOrderId} от {Source}",
					cancelOrderDto.ExternalOrderId,
					sourceName);

				return StatusCode(500, new
				{
					title = "One or more validation errors occurred",
					status = 500,
					detail = "Произошла ошибка, пожалуйста, попробуйте позже"
				});
			}
		}

		/*[HttpPost]
		public IActionResult UpdateOnlineOrderPaymentStatus(OnlineOrderPaymentStatusUpdatedDto paymentStatusUpdatedDto)
		{
			var sourceName = paymentStatusUpdatedDto.Source.GetEnumTitle();

			try
			{
				Logger.LogInformation(
					"Поступил запрос от {Source} на обновление статуса оплаты онлайн заказа {ExternalOrderId} c подписью {Signature}, проверяем...",
					sourceName,
					paymentStatusUpdatedDto.ExternalOrderId,
					paymentStatusUpdatedDto.Signature);
				
				if(!_customerOrdersService.ValidateOnlineOrderPaymentStatusUpdatedSignature(
					paymentStatusUpdatedDto, out var generatedSignature))
				{
					return InvalidSignature(orderRatingInfo.Signature, generatedSignature);
				}

				if(!_customerOrdersService.TryUpdateOnlineOrderPaymentStatus(paymentStatusUpdatedDto))
				{
					//возвращаем код не найденного заказа
					//return 
				}
				
				return Ok();
			}
			catch(Exception e)
			{
				Logger.LogError(
					e,
					"Ошибка при попытке обновить статус оплаты онлайн заказа {ExternalOrderId} от {Source}",
					paymentStatusUpdatedDto.ExternalOrderId,
					sourceName);
				return Problem();
			}
		}*/
	}
}
