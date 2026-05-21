using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace VodovozBusiness.Services.Sale
{
	/// <summary>
	/// Сервис по работе с правилами доставки
	/// </summary>
	public interface IDistrictRulesService
	{
		/// <summary>
		/// Получение списка правил доставки района в строковом представлении
		/// </summary>
		/// <param name="district">Логистический район</param>
		/// <param name="weekDay">День недели</param>
		/// <returns></returns>
		IList<string> GetDistrictRulesTitles(District district, WeekDayName weekDay);
		/// <summary>
		/// Получение графиков доставки
		/// </summary>
		/// <param name="district">Логистический район</param>
		/// <param name="weekDay">День недели</param>
		/// <param name="currentDate">Текущая дата и время</param>
		/// <param name="isStoppedOnlineDeliveriesToday">Остановлены онлайн доставки или нет</param>
		/// <returns></returns>
		IList<DeliverySchedule> GetScheduleRestrictions(
			District district,
			WeekDayName weekDay,
			DateTime currentDate,
			bool isStoppedOnlineDeliveriesToday);
		/// <summary>
		/// Сортировка по приоритетам:
		/// 1. Интервалы более 5 часов или ровно 5 часов, начинающиеся до 18:00
		/// 2. Интервалы более 5 часов или ровно 5 часов, начинающиеся после 18:00 или ровно в 18:00
		/// 3. Интервалы менее 5 часов, начинающиеся до 18:00
		/// 4. Интервалы менее 5 часов, начинающиеся после 18:00 или ровно в 18:00
		/// Также сортировка в каждой группе по величине интервала и если она совпадает, по первому времени интервала
		/// </summary>
		/// <param name="deliverySchedules">Графики доставки</param>
		/// <returns>Отсортированные графики доставки</returns>
		IEnumerable<DeliverySchedule> ReorderScheduleRestrictions(IList<DeliverySchedule> deliverySchedules);
	}
}
