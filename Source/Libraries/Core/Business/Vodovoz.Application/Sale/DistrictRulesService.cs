using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using VodovozBusiness.Services.Sale;

namespace Vodovoz.Application.Sale
{
	public class DistrictRulesService : IDistrictRulesService
	{
		/// <inheritdoc/>
		public IList<string> GetDistrictRulesTitles(District district, WeekDayName weekDay)
		{
			var weekDayRules = district.GetWeekDayRuleItemCollectionByWeekDayName(weekDay);

			if(weekDayRules.Any())
			{
				return weekDayRules
					.Select(x => x.Title)
					.ToList();
			}
			
			return district.CommonDistrictRuleItems
				.Select(x => x.Title)
				.ToList();
		}
		
		/// <inheritdoc/>
		public IList<DeliverySchedule> GetScheduleRestrictions(
			District district,
			WeekDayName weekDay,
			DateTime currentDate,
			bool isStoppedOnlineDeliveriesToday)
		{
			if(weekDay == WeekDayName.Today)
			{
				return isStoppedOnlineDeliveriesToday
					? new List<DeliverySchedule>()
					: district.GetScheduleRestrictionCollectionByWeekDayName(weekDay)
						.Where(x => x.AcceptBefore.Time > currentDate.TimeOfDay)
						.Select(x => x.DeliverySchedule)
						.ToList();
			}

			return GetScheduleRestrictionsByDate(district, weekDay, currentDate);
		}
		
		/// <inheritdoc/>
		public IEnumerable<DeliverySchedule> ReorderScheduleRestrictions(IList<DeliverySchedule> deliverySchedules)
		{
			// Cуществует интервал с 17 до 19,  на него правила сортировки не должны действовать, этот интервал должен отображаться в самом конце в любом случае.
			// (В дальнейшем появление подобных уникальных интервалов не планируется).
			var valuesFrom17To19 = deliverySchedules
				.Where(x => x.From == TimeSpan.FromHours(17) && x.To == TimeSpan.FromHours(19))
				.ToList();

			var result = deliverySchedules
				.Where(x => x.From != TimeSpan.FromHours(17) || x.To != TimeSpan.FromHours(19))
				.OrderBy(ds => ds.From)
				.ThenByDescending(ds => ds.To)
				.ToList();

			result.AddRange(valuesFrom17To19);

			return result;
		}
		
		/// <summary>
		/// Формирует список доступных интервалов доставки на день недели
		/// Если день недели - следующий день после времени запроса, то выбираем только те интервалы у которых
		/// либо не заполнено время приема до предыдущего дня заказа
		/// либо время приема до предыдущего дня больше текущего времени
		/// Иначе берем все интервалы дня недели
		/// Т.е. если запрос поступил в понедельник в 17:30, то на вторник мы отправляем интервалы у которых не заполнено время приема
		/// и где время больше 17:30
		/// Показатель следующего дня вычисляется через разность дня недели на который надо отправить интервалы и текущего дня
		/// разница между ними всегда будет равна 1, кроме случая когда текущий день - воскресение.
		/// </summary>
		/// <param name="district">Район</param>
		/// <param name="deliveryWeekDay">День недели, на который нужно отправить доступные интервалы доставки</param>
		/// <param name="currentDate">Текущее время(когда пришел запрос)</param>
		/// <returns>Список доступных интервалов доставки на день недели</returns>
		private IList<DeliverySchedule> GetScheduleRestrictionsByDate(
			District district,
			WeekDayName deliveryWeekDay,
			DateTime currentDate)
		{
			var dayOfWeek = District.ConvertDayOfWeekToWeekDayName(currentDate.DayOfWeek);

			if((deliveryWeekDay - dayOfWeek == 1) || (dayOfWeek == WeekDayName.Sunday && deliveryWeekDay == WeekDayName.Monday))
			{
				return district
					.GetScheduleRestrictionCollectionByWeekDayName(deliveryWeekDay)
					.Where(x => x.AcceptBefore == null || x.AcceptBefore.Time > currentDate.TimeOfDay)
					.Select(x => x.DeliverySchedule)
					.ToList();
			}

			return GetScheduleRestrictionsForWeekDay(district, deliveryWeekDay);
		}
		
		private IList<DeliverySchedule> GetScheduleRestrictionsForWeekDay(District district, WeekDayName weekDay)
		{
			return district
				.GetScheduleRestrictionCollectionByWeekDayName(weekDay)
				.Select(x => x.DeliverySchedule)
				.ToList();
		}
	}
}
