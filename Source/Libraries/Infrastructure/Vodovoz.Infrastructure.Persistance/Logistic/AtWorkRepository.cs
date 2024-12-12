using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Infrastructure.Persistance.Logistic
{
	internal sealed class AtWorkRepository : IAtWorkRepository
	{
		public IList<AtWorkDriver> GetDriversAtDay(IUnitOfWork uow, DateTime date, IEnumerable<AtWorkDriver.DriverStatus> driverStatuses = null, IEnumerable<CarTypeOfUse> carTypesOfUse = null,
			 IEnumerable<CarOwnType> carOwnTypes = null, int[] geoGroupIds = null)
		{
			AtWorkDriver atWorkDriverAlias = null;
			Car carAlias = null;
			CarVersion carVersionAlias = null;

			var query = QueryOver.Of(() => atWorkDriverAlias)
				.Left.JoinAlias(() => atWorkDriverAlias.Car, () => carAlias)
				.Left.JoinAlias(() => carAlias.CarVersions, () => carVersionAlias)
				.Where(x => x.Date == date);

			if(driverStatuses != null)
			{
				query.WhereRestrictionOn(awd => awd.Status).IsIn(driverStatuses.ToArray());
			}

			if(carTypesOfUse != null)
			{
				query
					.JoinQueryOver(awd => awd.Car)
					.JoinQueryOver(c => c.CarModel)
					.WhereRestrictionOn(cm => cm.CarTypeOfUse).IsIn(carTypesOfUse.ToArray());
			}

			if(carOwnTypes != null)
			{
				query
					.Where(() => carVersionAlias.StartDate <= date)
					.Where(Restrictions.Or(
						Restrictions.Where(() => carVersionAlias.EndDate == null),
						Restrictions.Where(() => carVersionAlias.EndDate >= date)))
					.WhereRestrictionOn(() => carVersionAlias.CarOwnType).IsIn(carOwnTypes.ToArray());
			}

			if(geoGroupIds != null)
			{
				query.WhereRestrictionOn(awd => awd.GeographicGroup.Id).IsIn(geoGroupIds);
			}

			query.Select(Projections.Distinct(Projections.Id()));

			AtWorkDriver atWorkDriverAlias2 = null;

			var resultQuery = uow.Session.QueryOver(() => atWorkDriverAlias2)
				.WithSubquery.WhereProperty(() => atWorkDriverAlias2.Id).In(query)
				.Fetch(SelectMode.Fetch, () => atWorkDriverAlias2.Employee)
				.Fetch(SelectMode.Fetch, () => atWorkDriverAlias2.Car);

			return resultQuery.List();
		}

		public IList<AtWorkForwarder> GetForwardersAtDay(IUnitOfWork uow, DateTime date)
		{
			return uow.Session.QueryOver<AtWorkForwarder>()
					  .Where(x => x.Date == date)
					  .Fetch(SelectMode.Fetch, x => x.Employee)
					  .List();
		}
	}
}
