using System;
using System.Collections.Generic;
using CustomerOrders.Contracts.V8.Sale.Templates;
using DateTimeHelpers;
using Vodovoz.Settings.Sale;
using VodovozBusiness.Services.Sale;

namespace Vodovoz.Core.Application.Sale
{
	public class TemplateNextDeliveryDateGenerator : ITemplateNextDeliveryDateGenerator
	{
		private readonly IOnlineOrderTemplateSettings _templateSettings;

		public TemplateNextDeliveryDateGenerator(IOnlineOrderTemplateSettings templateSettings)
		{
			_templateSettings = templateSettings ?? throw new ArgumentNullException(nameof(templateSettings));
		}

		public DateTime GetNextDeliveryDate(
			CustomerTemplateDeliveryScheduleType scheduleType,
			DateTime? firstDateTime,
			int? repeatEveryWeeks,
			int? dayOfMonth
		)
		{
			if(scheduleType == CustomerTemplateDeliveryScheduleType.Monthly)
			{
				var year = DateTime.Today.Year;
				var month = GetMonth(dayOfMonth.Value);
				return new DateTime(year, month, dayOfMonth.Value);
			}
			
			return GetNearestDeliveryDate(firstDateTime.Value, repeatEveryWeeks.Value);
		}
		
		public IEnumerable<DateTime> GetTemplateNextDeliveryDates(
			DateTime firstDateTime,
			int repeatEveryWeeks
			)
		{
			var countDates = _templateSettings.GetNumberNextDeliveriesToDisplay;
			var dates = new List<DateTime>();
			var startDate = GetNearestDeliveryDate(firstDateTime, repeatEveryWeeks);
			
			for(var i = 0; i < countDates; i++)
			{
				var date = startDate.AddWeeks(i * repeatEveryWeeks);
				dates.Add(date);
			}
			
			return dates;
		}

		public IEnumerable<DateTime> GetTemplateNextDeliveryDates(int dayOfMonth)
		{
			var countDates = _templateSettings.GetNumberNextDeliveriesToDisplay;
			var dates = new List<DateTime>();
			var month = GetMonth(dayOfMonth);
			var year = DateTime.Today.Year;

			for(var i = 0; i < countDates; i++)
			{
				//TODO сделать проверки по дате месяца, если ее нет то сдвигать по логике задачи
				var date = new DateTime(year, month, dayOfMonth);
				dates.Add(date);
				month++;
			}
			
			return dates;
		}

		private DateTime GetNearestDeliveryDate(DateTime firstDateTime, int repeatEveryWeeks)
		{
			if(firstDateTime >= DateTime.Today)
			{
				return firstDateTime;
			}
			
			var differenceDays = (DateTime.Today - firstDateTime).Days;
			var frequency = DateTimeExtensions.DaysInWeek() * repeatEveryWeeks;
			var multiplier = (int)Math.Ceiling(differenceDays / (decimal)frequency);
			
			return firstDateTime.AddDays(multiplier * frequency);
		}
		
		private int GetMonth(int dayOfMonth)
		{
			return DateTime.Today.Day >= dayOfMonth
				? DateTime.Today.Month
				: DateTime.Today.AddMonths(1).Month;
		}
	}
}
