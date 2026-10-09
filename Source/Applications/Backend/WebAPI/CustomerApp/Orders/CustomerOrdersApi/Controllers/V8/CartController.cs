using System;
using System.Net.Mime;
using System.Threading.Tasks;
using CustomerOrders.Contracts.V8.Carts;
using CustomerOrdersApi.Library.V8.Dto.Carts;
using CustomerOrdersApi.Library.V8.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Presentation.WebApi.Messages;

namespace CustomerOrdersApi.Controllers.V8
{
	[ApiVersion("8.0")]
	[Authorize]
	public class CartController : VersionedController
	{
		private readonly ICustomerCartService _customerCartService;

		public CartController(
			ILogger<CartController> logger,
			ICustomerCartService customerCartService
			) : base(logger)
		{
			_customerCartService = customerCartService ?? throw new ArgumentNullException(nameof(customerCartService));
		}
		
		/// <summary>
		/// Получение условий для дальнейшего оформления заказа
		/// </summary>
		/// <param name="request">Данные заказа из корзины для проверки <see cref="OrderConditionsRequest"/></param>
		/// <returns>Результат проверки <see cref="OrderConditionsResponse"/></returns>
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderConditionsResponse))]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[HttpPost]
		public async Task<IActionResult> GetOrderConditions(OrderConditionsRequest request)
		{
			try
			{
				_logger.LogInformation("Поступил запрос получения форм оплат и доп условий по заказу из корзины {@OrdersConditionsRequest}", request);

				var result = await _customerCartService.GetOrderConditionsAsync(request);
				return Ok(result);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при получении форм оплат и доп условий по заказу из корзины {ExternalCounterpartyId} от {Source}",
					request.ExternalCounterpartyId,
					request.Source.ToString());
				
				return Problem(ResponseMessage.HasErrorOccurredPleaseTryAgainLater);
			}
		}
	}
}
