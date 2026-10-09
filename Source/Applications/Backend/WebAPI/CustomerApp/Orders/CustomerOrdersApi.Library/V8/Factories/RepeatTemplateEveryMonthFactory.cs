using System;
using System.Collections.Generic;
using System.Linq;
using CustomerApp.Contracts.Sale.Templates;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Settings.Sale;

namespace CustomerOrdersApi.Library.V8.Factories
{
	/// <inheritdoc/>
	public class RepeatTemplateEveryMonthFactory : IRepeatTemplateEveryMonthFactory
	{
		private readonly IOnlineOrderTemplateSettings _templateSettings;

		public RepeatTemplateEveryMonthFactory(
			IOnlineOrderTemplateSettings templateSettings
			)
		{
			_templateSettings = templateSettings ?? throw new ArgumentNullException(nameof(templateSettings));
		}
		
		/// <inheritdoc/>
		public TemplateMonthlySchedule Create(ILookup<WeekDayName, DeliveryScheduleDto> scheduleLookup)
		{
			if(scheduleLookup is null)
			{
				return TemplateMonthlySchedule.CreateNotAvailable();
			}
			
			var firstDaySchedules = scheduleLookup.FirstOrDefault();

			if(!firstDaySchedules.Any())
			{
				return TemplateMonthlySchedule.CreateNotAvailable();
			}
			
			var commonSchedules = firstDaySchedules
				.Select(x => x)
				.ToList();
			
			var processResult = ProcessSchedules(scheduleLookup, commonSchedules);

			if(!processResult)
			{
				return TemplateMonthlySchedule.CreateNotAvailable();
			}
			
			var monthDays = new List<MonthDayAvailability>();
			var min = _templateSettings.MonthDayMin;
			var max = _templateSettings.MonthDayMax;
			
			for(var i = min; i <= max; i++)
			{
				monthDays.Add(MonthDayAvailability.CreateAvailable(i));
			}

			return TemplateMonthlySchedule.CreateAvailable(commonSchedules, monthDays);
		}

		private bool ProcessSchedules(
			ILookup<WeekDayName, DeliveryScheduleDto> scheduleLookup,
			IList<DeliveryScheduleDto> commonSchedules
			)
		{
			var i = 0;
			
			foreach(var groupedWeeklySchedules in scheduleLookup)
			{
				if(i == 0)
				{
					i++;
					continue;
				}

				var tempCommonSchedules = commonSchedules
					.Intersect(groupedWeeklySchedules.ToList())
					.ToList();

				//если нет пересечений хотя бы по одному дню, то бракуем
				if(!tempCommonSchedules.Any())
				{
					return false;
				}
				
				commonSchedules = tempCommonSchedules;
				i++;
			}
			
			return true;
		}
	}
}
