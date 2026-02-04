using CustomerOrdersApi.Library.Dto.Orders;
using CustomerOrdersApi.Library.Services;
using Gamma.Utilities;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VodovozHealthCheck.Helpers;

namespace CustomerOrdersApi.Controllers
{
	public class OrdersController : SignatureControllerBase
	{
		private readonly ICustomerOrdersService _customerOrdersService;
		private readonly IPublishEndpoint _publishEndpoint;

		public OrdersController(
			ILogger<OrdersController> logger,
			ICustomerOrdersService customerOrdersService,
			IPublishEndpoint publishEndpoint
			) : base(logger)
		{
			_customerOrdersService = customerOrdersService ?? throw new ArgumentNullException(nameof(customerOrdersService));
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
