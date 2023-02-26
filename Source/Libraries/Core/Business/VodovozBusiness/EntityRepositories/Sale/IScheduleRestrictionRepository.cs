using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sale;

namespace Vodovoz.EntityRepositories.Sale
{
	public interface IScheduleRestrictionRepository
	{
		int GetDistrictsForFastDeliveryCurrentVersionId(IUnitOfWork uow);
		int GetDistrictsForFastDeliveryHistoryVersionId(IUnitOfWork unitOfWork, DateTime dateTime);
		QueryOver<District> GetDistrictsWithBorder();
		IList<District> GetDistrictsWithBorder(IUnitOfWork uow);
		IList<District> GetDistrictsWithBorderForFastDelivery(IUnitOfWork uow);
		IList<District> GetDistrictsWithBorderForFastDeliveryAtDateTime(IUnitOfWork uow, DateTime dateTime);
		IEnumerable<OrderCountResultNode> OrdersCountByDistrict(IUnitOfWork uow, DateTime date, int minBottlesInOrder);
	}
}
