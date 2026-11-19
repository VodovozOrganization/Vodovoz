using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.BasicHandbooks
{
	public interface IDeliveryScheduleRepository
	{
		QueryOver<DeliverySchedule> AllQuery();
		QueryOver<DeliverySchedule> NotArchiveQuery();
		IList<DeliverySchedule> All(IUnitOfWork uow);
		/// <summary>
		/// Получение графика доставки
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="deliveryScheduleId">Идентификатор графика доставки</param>
		/// <returns></returns>
		DeliverySchedule Get(IUnitOfWork uow, int deliveryScheduleId);
		/// <summary>
		/// Проверка существования графика доставки
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="deliveryScheduleId">Идентификатор графика доставки</param>
		/// <returns></returns>
		bool Exists(IUnitOfWork uow, int deliveryScheduleId);

		/// <summary>
		/// График доставки доступен
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="weekDay">День недели</param>
		/// <param name="deliveryPointId">Идентификатор ТД</param>
		/// <param name="deliveryScheduleId">Идентификатор интервала доставки</param>
		/// <returns></returns>
		bool Available(IUnitOfWork uow, DayOfWeek weekDay, int deliveryPointId, int deliveryScheduleId);
		
		/// <summary>
		/// Получение дней недели с доступным интервалом доставки, который ищем 
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="deliveryWeekDay">День недели доставки</param>
		/// <param name="templateWeekDay">День недели доставки из шаблона</param>
		/// <param name="deliveryPointId">Идентификатор ТД</param>
		/// <param name="deliveryScheduleId">Идентификатор интервала доставки</param>
		/// <returns></returns>
		ILookup<DayOfWeek, int> GetWeekdaysWithDeliveryInterval(
			IUnitOfWork uow,
			DayOfWeek deliveryWeekDay,
			DayOfWeek templateWeekDay,
			int deliveryPointId,
			int deliveryScheduleId
			);
	}
}
