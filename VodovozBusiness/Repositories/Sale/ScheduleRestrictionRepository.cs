using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QSOrmProject;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Repositories.Sale
{
	public static class ScheduleRestrictionRepository
	{
		public static QueryOver<ScheduleRestrictedDistrict> AreaWithGeometryQuery()
		{
			return QueryOver.Of<ScheduleRestrictedDistrict>().Where(x => x.DistrictBorder != null);
		}

		public static IList<ScheduleRestrictedDistrict> AreaWithGeometry(IUnitOfWork uow)
		{
			return AreaWithGeometryQuery()
							.GetExecutableQueryOver(uow.Session)
							.List();
		}
	}
}
