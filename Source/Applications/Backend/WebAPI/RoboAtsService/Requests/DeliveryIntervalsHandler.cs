﻿using Microsoft.Extensions.Logging;
using RoboatsService.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Roboats;
using Vodovoz.EntityRepositories.Roboats;

namespace RoboatsService.Requests
{
	/// <summary>
	/// Обработчик запросов получения интервалов доставки
	/// </summary>
	public class DeliveryIntervalsHandler : GetRequestHandlerBase
	{
		private readonly ILogger<DeliveryIntervalsHandler> _logger;
		private readonly IRoboatsRepository _roboatsRepository;
		private readonly RoboatsCallRegistrator _callRegistrator;

		public override string Request => RoboatsRequestType.DateTime;

		public DeliveryIntervalsHandler(ILogger<DeliveryIntervalsHandler> logger, IRoboatsRepository roboatsRepository, RequestDto requestDto, RoboatsCallRegistrator callRegistrator) : base(requestDto)
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
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.Exception, RoboatsCallOperation.OnDeliveryIntervalsHandle,
						$"При обработке запроса информации об интервалах доставки возникло исключение: {ex.Message}. Обратитесь в отдел разработки.");
				return ErrorMessage;
			}
		}

		public string ExecuteRequest()
		{
			var intervalRestrictions = _roboatsRepository.GetRoboatsDeliveryIntervalRestrictions();
			var intervals = _roboatsRepository.GetRoboatsAvailableDeliveryIntervals();
			if(!intervals.Any())
			{
				_callRegistrator.RegisterFail(ClientPhone, RequestDto.CallGuid, RoboatsCallFailType.DeliveryIntervalsNotFound, RoboatsCallOperation.GetDeliveryIntervals,
					$"Не найдены интервалы доставки. Проверьте справочник интервалов доставки для Roboats.");
				return "NO DATA";
			}

			string todayIntervals = GetTodayIntervals(intervals);

			string result = todayIntervals;

			var dates = new List<DateTime>();
			dates.Add(DateTime.Today.AddDays(1));
			foreach(var date in dates)
			{
				if(!string.IsNullOrWhiteSpace(result))
				{
					result += "|";
				}

				if(DateTime.Today.AddDays(1) == date)
				{
					var availableIntervals = intervals.ToList();

					var deniedIntervalIds = intervalRestrictions.Where(x => x.BeforeAcceptOrderHour <= DateTime.Now.Hour)
						.Select(x => x.DeliverySchedule.Id);

					foreach(var interval in intervals)
					{
						if(deniedIntervalIds.Contains(interval.Id))
						{
							availableIntervals.Remove(interval);
						}
					}

					intervals = availableIntervals;
				}


				var availableIntervalIds = intervals.Select(x => x.Id);
				result += GetOutputIntervalString(date, availableIntervalIds);
			}

			return result;
		}

		private string GetTodayIntervals(IEnumerable<DeliverySchedule> intervals)
		{

			IEnumerable<int> availableIntevalIds = null;

			if(DateTime.Now.Hour < 12)
			{
				availableIntevalIds = intervals.Where(x => x.From.Hours > 12).Select(x => x.Id);
			}
			else if(DateTime.Now.Hour < 17)
			{
				availableIntevalIds = intervals.Where(x => x.From.Hours > 17).Select(x => x.Id);
			}
			else
			{
				return string.Empty;
			}

			var result = GetOutputIntervalString(DateTime.Today, availableIntevalIds);
			return result;
		}

		private string GetOutputIntervalString(DateTime date, IEnumerable<int> intervals)
		{
			var formattedDate = date.ToString("yyyy-MM-dd");
			return string.Join('|', intervals.Select(x => $"{formattedDate}\\{x}"));
		}
	}
}
