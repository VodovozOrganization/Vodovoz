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
		/// Получение графика доставки в виде ноды под интерфейсом <see cref="INamedDomainObject"/>
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="deliveryScheduleId">Идентификатор графика доставки</param>
		/// <returns></returns>
		INamedDomainObject Get(IUnitOfWork uow, int deliveryScheduleId);
		/// <summary>
		/// Проверка существования графика доставки
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="deliveryScheduleId">Идентификатор графика доставки</param>
		/// <returns></returns>
		bool Exists(IUnitOfWork uow, int deliveryScheduleId);
	}
}
