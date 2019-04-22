using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Repositories.Sale
{
	public static class ScheduleRestrictionRepository
	{
		public static QueryOver<ScheduleRestrictedDistrict> AreaWithGeometryQuery()
		{
			return QueryOver.Of<ScheduleRestrictedDistrict>().Where(x => x.DistrictBorder != null);
		}

		public static IList<ScheduleRestrictedDistrict> AreasWithGeometry(IUnitOfWork uow)
		{
			return AreaWithGeometryQuery()
							.GetExecutableQueryOver(uow.Session)
							.List();
		}
	}
}