using CustomerOrdersApi.Library.Config;
using CustomerOrdersApi.Library.Dto.Orders;
using CustomerOrdersApi.Library.Services;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using Vodovoz.Core.Domain.Clients;
using VodovozHealthCheck.Helpers;

namespace CustomerOrdersApi.Controllers
{
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

				if(!orderRatingInfo.OrderId.HasValue && !orderRatingInfo.OnlineOrderId.HasValue)
				{
					Logger.LogWarning("Пришла оценка неизвестного заказа с {Source}", sourceName);
					return Problem("Произошла ошибка, пожалуйста, попробуйте позже");
				}
				
				var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);

				if(!isDryRun)
				{
					_customerOrdersService.CreateOrderRating(orderRatingInfo);
				}
				
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

				var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);

				if(!isDryRun && _memoryCache.TryGetValue(source, out var value))
				{
					return BadRequest("Превышен интервал обращений");
				}

				if(!isDryRun)
				{
					_memoryCache.Set(
					source,
					DateTime.Now,
					TimeSpan.FromMinutes(_requestsMinutesLimitsOptions.OrderRatingReasonsRequestFrequencyLimit));
				}
				
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
