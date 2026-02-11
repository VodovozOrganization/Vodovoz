using CustomerAppsApi.Library.Dto.Goods;
using CustomerAppsApi.Library.Models;
using Gamma.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using Vodovoz.Core.Domain.Clients;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/")]
	public class PromotionalSetController : ControllerBase
	{
		private readonly ILogger<CounterpartyController> _logger;
		private readonly IPromotionalSetModel _promotionalSetModel;
		private readonly IConfigurationSection _requestsLimitsSection;
		private static readonly ConcurrentDictionary<Source, DateTime> _requestTimes = new ConcurrentDictionary<Source, DateTime>();

		public PromotionalSetController(
			ILogger<CounterpartyController> logger,
			IPromotionalSetModel promotionalSetModel,
			IConfiguration configuration)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_promotionalSetModel = promotionalSetModel ?? throw new ArgumentNullException(nameof(promotionalSetModel));
			_requestsLimitsSection =
				(configuration ?? throw new ArgumentNullException(nameof(configuration)))
				.GetSection("RequestsMinutesLimits");
		}

		[HttpGet("GetPromotionalSets")]
		public PromotionalSetsDto GetPromotionalSets([FromQuery] Source source)
		{
			var sourceName = source.GetEnumTitle();
			try
			{
				_logger.LogInformation("Поступил запрос на выборку промонаборов от источника {Source}", sourceName);
				var now = DateTime.Now;
				var lastRequestTime = _requestTimes.GetOrAdd(source, now);
				var passedTimeMinutes = lastRequestTime == now ? 0d : (now - lastRequestTime).TotalMinutes;

				var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);

				var canRequest = isDryRun
					|| passedTimeMinutes == 0
					|| passedTimeMinutes >= _requestsLimitsSection.GetValue<int>("PromotionalSetsRequestFrequencyLimit");

				if(!canRequest)
				{
					_logger.LogInformation("Превышен интервал обращений для источника {Source}", sourceName);
					return new PromotionalSetsDto
					{
						ErrorMessage = "Превышен интервал обращений"
					};
				}

				var promotionalSets = _promotionalSetModel.GetPromotionalSets(source);

				if(!isDryRun)
				{
					_requestTimes.TryUpdate(source, DateTime.Now, lastRequestTime);
				}

				return promotionalSets;
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при получении промонаборов для источника {Source}", sourceName);
				return new PromotionalSetsDto
				{
					ErrorMessage = e.Message
				};
			}
		}
	}
}
