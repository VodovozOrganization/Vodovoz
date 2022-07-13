using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.EntityRepositories.Counterparties;

namespace RoboAtsService.Requests
{
	/// <summary>
	/// Обработчик запросов получения интервалов доставки
	/// </summary>
	public class DeliveryIntervalsHandler : GetRequestHandlerBase
	{
		private readonly RoboatsRepository _roboatsRepository;

		public override string Request => RoboatsRequestType.DateTime;

		public DeliveryIntervalsHandler(RoboatsRepository roboatsRepository, RequestDto requestDto) : base(requestDto)
		{
			_roboatsRepository = roboatsRepository ?? throw new ArgumentNullException(nameof(roboatsRepository));
		}

		public override string Execute()
		{
			var counterpartyIds = _roboatsRepository.GetCounterpartyIdsByPhone(ClientPhone);
			if(counterpartyIds.Count() != 1)
			{
				return ErrorMessage;
			}

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
				return "NO DATA";
			}
		}
	}
}
