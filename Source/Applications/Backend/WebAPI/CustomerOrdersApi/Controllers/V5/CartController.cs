using System;
using System.Net.Mime;
using CustomerOrdersApi.Library.V5.Dto.Carts;
using CustomerOrdersApi.Library.V5.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Presentation.WebApi.Messages;

namespace CustomerOrdersApi.Controllers.V5
{
	[ApiVersion("5.0")]
	[Authorize]
	public class CartController : VersionedController
	{
		private readonly ICustomerCartService _customerCartService;

		public CartController(
			ILogger<CartController> logger,
			ICustomerCartService customerCartService) : base(logger)
		{
			_customerCartService = customerCartService ?? throw new ArgumentNullException(nameof(customerCartService));
		}
		
		/// <summary>
		/// Проверка корзины
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CheckUsersBasketResponse))]
		[HttpPost]
		public IActionResult CheckUsersBasket(CheckUsersBasketRequest request)
		{
			try
			{
				_logger.LogInformation("Поступил запрос проверки корзины {@CheckUsersBasketRequest}", request);

				var result = _customerCartService.Check(request);
				return Ok(result);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при проверке корзины пользователя {ExternalCounterpartyId} от {Source}",
					request.ExternalCounterpartyId,
					request.Source.ToString());
				
				return Problem(ResponseMessage.HasErrorOccurredPleaseTryAgainLater);
			}
		}
	}
}
