using Microsoft.Extensions.Logging;
using RoboAtsService.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Roboats;
using Vodovoz.EntityRepositories.Counterparties;

namespace RoboAtsService.Requests
{
	/// <summary>
	/// Обработчик запросов получения интервалов доставки
	/// </summary>
	public class DeliveryIntervalsHandler : GetRequestHandlerBase
	{
		private readonly ILogger<DeliveryIntervalsHandler> _logger;
		private readonly RoboatsRepository _roboatsRepository;
		private readonly RoboatsCallRegistrator _callRegistrator;

		public override string Request => RoboatsRequestType.DateTime;

		public DeliveryIntervalsHandler(ILogger<DeliveryIntervalsHandler> logger, RoboatsRepository roboatsRepository, RequestDto requestDto, RoboatsCallRegistrator callRegistrator) : base(requestDto)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
			_callRegistrator = callRegistrator ?? throw new ArgumentNullException(nameof(callRegistrator));
		}

		public override string Execute()
		{
			try
			{
				return ExecuteRequest();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "При обработке запроса информации об интервалах доставки возникло исключение");
				_callRegistrator.RegisterFail(ClientPhone, RoboatsCallFailType.Exception, RoboatsCallOperation.OnDeliveryIntervalsHandle,
						$"При обработке запроса информации об интервалах доставки возникло исключение: {ex.Message}");
				return ErrorMessage;
			}
		}

		public string ExecuteRequest()
		{
			var intervals = _roboatsRepository.GetRoboatsAvailableDeliveryIntervals();
			if(intervals.Any())
			{
				var dates = new List<DateTime>();
				if(DateTime.Now.Hour < 12)
				{
					dates.Add(DateTime.Today);
				}
				dates.Add(DateTime.Today.AddDays(1));

				string result = "";

				foreach(var date in dates)
				{
					if(!string.IsNullOrWhiteSpace(result))
					{
						result += "|";
					}
					var formattedDate = date.ToString("yyyy-MM-dd");
					result += string.Join('|', intervals.Select(x => $"{formattedDate}\\{x}"));
				}

				return result;
			}
			else
			{
				_callRegistrator.RegisterFail(ClientPhone, RoboatsCallFailType.DeliveryIntervalsNotFound, RoboatsCallOperation.GetDeliveryIntervals,
					$"Не найдены интервалы доставки");
				return "NO DATA";
			}
		}
	}
}
