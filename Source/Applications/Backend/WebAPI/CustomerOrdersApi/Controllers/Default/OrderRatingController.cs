using System;
using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.Default.Dto.Orders;
using CustomerOrdersApi.Library.Default.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Controllers.Default
{
	[ApiVersion("3.0")]
	public class OrderRatingController : SignatureControllerBase
	{
		private readonly ICustomerOrdersService _customerOrdersService;
		private readonly IMemoryCache _memoryCache;
		private readonly RequestsMinutesLimitsOptions _requestsMinutesLimitsOptions;

		public OrderRatingController(
			ILogger<OrderRatingController> logger,
			ICustomerOrdersService customerOrdersService,
			IOptions<RequestsMinutesLimitsOptions> requestsLimitsOptions,
			IMemoryCache memoryCache) : base(logger)
		{
			_customerOrdersService = customerOrdersService ?? throw new ArgumentNullException(nameof(customerOrdersService));
			_memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
			_requestsMinutesLimitsOptions =
				(requestsLimitsOptions ?? throw new ArgumentNullException(nameof(requestsLimitsOptions)))
				.Value;
		}
		
		[HttpPost]
		public IActionResult CreateOrderRating(OrderRatingInfoForCreateDto orderRatingInfo)
		{
			var sourceName = orderRatingInfo.Source.GetEnumTitle();

			try
			{
				Logger.LogInformation(
					"Поступил запрос от {Source} на регистрацию оценки заказа {OrderId} c подписью {Signature}, проверяем...",
					sourceName,
					orderRatingInfo.OnlineOrderId,
					orderRatingInfo.Signature);
				
				if(!_customerOrdersService.ValidateOrderRatingSignature(orderRatingInfo, out var generatedSignature))
				{
					return InvalidSignature(orderRatingInfo.Signature, generatedSignature);
				}
				
				_customerOrdersService.CreateOrderRating(orderRatingInfo);
				return Ok();
			}
			catch(Exception e)
			{
				Logger.LogError(
					e,
					"Ошибка при попытке оценки заказа {OnlineOrderId} от {Source}",
					orderRatingInfo.OnlineOrderId,
					sourceName);
				return Problem();
			}
		}
		
		[HttpGet]
		public IActionResult GetOrderRatingReasons(Source source)
		{
			var sourceName = source.GetEnumTitle();

			try
			{
				Logger.LogInformation("Пришел запрос на получение всех причин оценки заказа от {Source}", sourceName);

				if(_memoryCache.TryGetValue(source, out var value))
				{
					return BadRequest("Превышен интервал обращений");
				}

				_memoryCache.Set(
					source,
					DateTime.Now,
					TimeSpan.FromMinutes(_requestsMinutesLimitsOptions.OrderRatingReasonsRequestFrequencyLimit));
				
				var reasons = _customerOrdersService.GetOrderRatingReasons();
				return Ok(reasons);
			}
			catch(Exception e)
			{
				Logger.LogError(e, "Ошибка при получении причин оценок заказа от {Source}", sourceName);
				return Problem();
			}
		}
	}
}
