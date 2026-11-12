using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
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
	}
}
