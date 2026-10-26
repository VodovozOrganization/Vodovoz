using System;
using System.Collections.Generic;
using CustomerOrders.Contracts.V8.Sale.Templates;

namespace VodovozBusiness.Services.Sale
{
	public interface ITemplateNextDeliveryDateGenerator
	{
		/// <summary>
		/// Получение следующей даты доставки автозаказа
		/// </summary>
		/// <param name="scheduleType">Тип периодичности доставки</param>
		/// <param name="firstDateTime">Первая дата доставки</param>
		/// <param name="repeatEveryWeeks">Периодичность в неделях(раз в неделю, раз в две, три и т.д.)</param>
		/// <param name="dayOfMonth">Число месяца</param>
		/// <returns></returns>
		DateTime GetNextDeliveryDate(
			CustomerTemplateDeliveryScheduleType scheduleType,
			DateTime? firstDateTime,
			int? repeatEveryWeeks,
			int? dayOfMonth
		);

		/// <summary>
		/// Получение списка следующих дат доставок автозаказа при недельной периодичности
		/// </summary>
		/// <param name="firstDateTime">Первая дата доставки</param>
		/// <param name="repeatEveryWeeks">Периодичность в неделях(раз в неделю, раз в две, три и т.д.)</param>
		/// <returns></returns>
		IEnumerable<DateTime> GetTemplateNextDeliveryDates(
			DateTime firstDateTime,
			int repeatEveryWeeks
		);

		/// <summary>
		/// Получение списка следующих дат доставок автозаказа при месячной периодичности
		/// </summary>
		/// <param name="dayOfMonth">Число месяца</param>
		/// <returns></returns>
		IEnumerable<DateTime> GetTemplateNextDeliveryDates(int dayOfMonth);
	}
}
