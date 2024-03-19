using System;
using System.Threading.Tasks;
using CustomerOrdersApi.Library.Dto.Orders;
using CustomerOrdersApi.Library.Services;
using Gamma.Utilities;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerOrdersApi.Controllers
{
	[ApiController]
	[Route("/api/")]
	public class OrdersController : ControllerBase
	{
		private readonly ILogger<OrdersController> _logger;
		private readonly ICustomerOrdersService _customerOrdersService;
		private readonly IPublishEndpoint _publishEndpoint;

		public OrdersController(
			ILogger<OrdersController> logger,
			ICustomerOrdersService customerOrdersService,
			IPublishEndpoint publishEndpoint
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_customerOrdersService = customerOrdersService ?? throw new ArgumentNullException(nameof(customerOrdersService));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		[HttpPost("CreateOrder")]
		public async Task<IActionResult> CreateOrderAsync(OnlineOrderInfoDto onlineOrderInfoDto)
		{
			var sourceName = onlineOrderInfoDto.Source.GetEnumTitle();
			
			try
			{
				_logger.LogInformation(
					"Поступил запрос от {Source} на регистрацию заказа для {CounterpartyId} c подписью {Signature}, проверяем...",
					sourceName,
					onlineOrderInfoDto.CounterpartyErpId,
					onlineOrderInfoDto.Signature);
				
				/*if(!_customerOrdersService.ValidateSignature(onlineOrderInfoDto, out var generatedSignature))
				{
					const string invalidSignature = "Неккоректная подпись онлайн заказа";
					_logger.LogWarning(
						"{InvalidSignature}. Пришла {ReceivedSign}, должна быть {GeneratedSignature}",
						invalidSignature,
						onlineOrderInfoDto.Signature,
						generatedSignature
						);
					return ValidationProblem(invalidSignature);
				}*/

				_logger.LogInformation("Подпись валидна, отправляем в очередь");
				await _publishEndpoint.Publish(onlineOrderInfoDto);
				
				return Accepted();
			}
			catch(Exception e)
			{
				_logger.LogError(e,
					"Ошибка при регистрации заказа для клиента № {CounterpartyId} от {Source}",
					onlineOrderInfoDto.CounterpartyErpId,
					sourceName);

				return Problem();
			}
		}
	}
}
