using System;
using System.Collections.Generic;
using System.Linq;
using CustomerApp.Contracts.Sale.Templates;
using CustomerOrdersApi.Library.V8.Services;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Settings.Sale;

namespace CustomerOrdersApi.Library.V8.Factories
{
	/// <inheritdoc/>
	public class RepeatTemplateEveryMonthFactory : IRepeatTemplateEveryMonthFactory
	{
		private readonly IOnlineOrderTemplateSettings _templateSettings;
		private readonly CommonDeliverySchedulesService _commonDeliverySchedulesService;

		public RepeatTemplateEveryMonthFactory(
			IOnlineOrderTemplateSettings templateSettings,
			CommonDeliverySchedulesService commonDeliverySchedulesService
			)
		{
			_templateSettings = templateSettings ?? throw new ArgumentNullException(nameof(templateSettings));
			_commonDeliverySchedulesService =
				commonDeliverySchedulesService ?? throw new ArgumentNullException(nameof(commonDeliverySchedulesService));
		}
		
		/// <inheritdoc/>
		public TemplateMonthlySchedule Create(ILookup<WeekDayName, DeliveryScheduleDto> scheduleLookup)
		{
			if(scheduleLookup is null)
			{
				return TemplateMonthlySchedule.CreateNotAvailable();
			}
			
			var processResult = _commonDeliverySchedulesService.ProcessSchedules(scheduleLookup, out var commonSchedules);

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
	}
}
