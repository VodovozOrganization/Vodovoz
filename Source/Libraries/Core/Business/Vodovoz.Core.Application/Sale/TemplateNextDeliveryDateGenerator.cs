using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrders.Contracts.V8.Sale.Templates;
using DateTimeHelpers;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Settings.Sale;
using VodovozBusiness.Errors.Sale;
using VodovozBusiness.Services.Sale;

namespace Vodovoz.Core.Application.Sale
{
	public class TemplateNextDeliveryDateGenerator : ITemplateNextDeliveryDateGenerator
	{
		private readonly IOnlineOrderTemplateSettings _templateSettings;
		private readonly DeliveryScheduleChecker _deliveryScheduleChecker;

		public TemplateNextDeliveryDateGenerator(
			IOnlineOrderTemplateSettings templateSettings,
			DeliveryScheduleChecker deliveryScheduleChecker
			)
		{
			_templateSettings = templateSettings ?? throw new ArgumentNullException(nameof(templateSettings));
			_deliveryScheduleChecker = deliveryScheduleChecker ?? throw new ArgumentNullException(nameof(deliveryScheduleChecker));
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
				var nextDate = GetNearestDeliveryDate(dayOfMonth.Value);
				return nextDate;
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
			var firstDate = GetNearestDeliveryDate(dayOfMonth);
			var month = firstDate.Month;
			var year = firstDate.Year;

			dates.Add(firstDate);
			month++;

			for(var i = 1; i < countDates; i++)
			{
				if(month > 12)
				{
					month = 1;
					year++;
				}

				var firstDayInMonth = new DateTime(year, month, 1);
				DateTime nextDate;

				if(firstDayInMonth.GetDaysInMonth() > dayOfMonth)
				{
					nextDate = new DateTime(year, month, dayOfMonth);
					month++;
				}
				else
				{
					//если нет такого числа в месяце, сдвигаем вправо
					nextDate = new DateTime(year, month + 1, 1);
				}

				dates.Add(nextDate);
			}

			return dates;
		}

		public Result<IEnumerable<DateTime>> GetDatesForResumeTemplate(
			IUnitOfWork uow,
			DayOfWeek dayOfWeek,
			DateTime requestTime,
			int deliveryPointId,
			int deliveryScheduleId
			)
		{
			var countDates = _templateSettings.GetNumberNextDeliveriesToDisplayByWeekly;
			var dates = new List<DateTime>();
			var startDateResult = GetNearestDeliveryDate(uow, dayOfWeek, requestTime, deliveryPointId, deliveryScheduleId);

			if(startDateResult.IsFailure)
			{
				return Result.Failure<IEnumerable<DateTime>>(startDateResult.Errors);
			}

			var startDate = startDateResult.Value;
			
			for(var i = 0; i < countDates; i++)
			{
				var date = startDate.AddWeeks(i);
				dates.Add(date);
			}
			
			return Result.Success(dates.AsEnumerable());
		}

		public Result<IEnumerable<DateTime>> GetDatesForResumeTemplate(
			IUnitOfWork uow,
			int dayOfMonth,
			DateTime requestTime,
			int deliveryPointId,
			int deliveryScheduleId
			)
		{
			var countDates = _templateSettings.GetNumberNextDeliveriesToDisplayByMonthly;
			var dates = new List<DateTime>();
			var firstDate = GetNearestDeliveryDate(dayOfMonth, requestTime);

			if(!_deliveryScheduleChecker.IsDeliveryIntervalAvailable(uow, firstDate.DayOfWeek, deliveryPointId, deliveryScheduleId))
			{
				return dates;
			}
			
			var month = firstDate.Month;
			var year = firstDate.Year;

			dates.Add(firstDate);
			month++;

			for(var i = 1; i < countDates; i++)
			{
				if(month > 12)
				{
					month = 1;
					year++;
				}

				var firstDayInMonth = new DateTime(year, month, 1);
				DateTime nextDate;

				if(firstDayInMonth.GetDaysInMonth() > dayOfMonth)
				{
					nextDate = new DateTime(year, month, dayOfMonth);
					month++;
				}
				else
				{
					//если нет такого числа в месяце, сдвигаем вправо
					nextDate = new DateTime(year, month + 1, 1);
				}

				dates.Add(nextDate);
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
		
		private Result<DateTime> GetNearestDeliveryDate(
			IUnitOfWork uow,
			DayOfWeek templateWeekDay,
			DateTime requestTime,
			int deliveryPointId,
			int deliveryScheduleId
			)
		{
			var today = DateTime.Today;

			var possibleDate = today.AddDays(
				requestTime.TimeOfDay < TimeSpan.FromHours(11)
					? _templateSettings.GetAddDaysToDeliveryDateBefore11Hours
					: _templateSettings.GetAddDaysToDeliveryDateAfter11Hours
				);

			var possibleDay = possibleDate.DayOfWeek;
			DateTime foundDate;

			if(possibleDay <= templateWeekDay)
			{
				var addDays = templateWeekDay - possibleDay;
				foundDate = possibleDate.AddDays(addDays);
			}
			else
			{
				foundDate = possibleDate.AddDays(DateTimeExtensions.DaysInWeek() - (int)possibleDay + (int)templateWeekDay);
			}

			return _deliveryScheduleChecker.IsDeliveryIntervalAvailable(uow, templateWeekDay, deliveryPointId, deliveryScheduleId)
				? Result.Success(foundDate)
				: Result.Failure<DateTime>(OnlineOrderTemplateApiErrors.OrderTemplateIntervalUnavailable);
		}
		
		private DateTime GetNearestDeliveryDate(int dayOfMonth)
		{
			var today = DateTime.Today;

			if(DateTime.Today.Day >= dayOfMonth)
			{
				var firstDayInMonth = new DateTime(today.Year, today.Month, 1);
				var nextMonth = 0;
				var year = 0;

				if(today.Month == 12)
				{
					nextMonth = 1;
					year = today.Year + 1;
				}
				else
				{
					nextMonth = today.Month + 1;
					year = today.Year;
				}

				if(firstDayInMonth.GetDaysInMonth() < dayOfMonth)
				{
					return new DateTime(year, nextMonth, 1);
				}

				return new DateTime(year, nextMonth, dayOfMonth);
			}

			return new DateTime(today.Year, today.Month, dayOfMonth);
		}
		
		private DateTime GetNearestDeliveryDate(
			int dayOfMonth,
			DateTime requestTime
			)
		{
			//учет времени
			//успевает в сборку: при запросе до 11:00 — не раньше сегодня + 2 дня, после 11:00 — сегодня + 3 дня
			var addDays = requestTime.TimeOfDay < TimeSpan.FromHours(11)
				? _templateSettings.GetAddDaysToDeliveryDateBefore11Hours
				: _templateSettings.GetAddDaysToDeliveryDateAfter11Hours;
			
			var startDate = DateTime.Today.AddDays(addDays);

			if(startDate.Day > dayOfMonth)
			{
				var firstDayInMonth = new DateTime(startDate.Year, startDate.Month, 1);
				var nextMonth = 0;
				var year = 0;

				if(startDate.Month == 12)
				{
					nextMonth = 1;
					year = startDate.Year + 1;
				}
				else
				{
					nextMonth = startDate.Month + 1;
					year = startDate.Year;
				}
	
				if(firstDayInMonth.GetDaysInMonth() < dayOfMonth)
				{
					return new DateTime(year, nextMonth, 1);
				}
				
				return new DateTime(year, nextMonth, dayOfMonth);
			}

			return new DateTime(startDate.Year, startDate.Month, dayOfMonth);
		}
	}
}
