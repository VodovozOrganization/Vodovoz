using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Sale;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class AtWorkRepository : IAtWorkRepository
	{
		public IList<AtWorkDriver> GetDriversAtDay(IUnitOfWork uow, DateTime date, IEnumerable<AtWorkDriver.DriverStatus> driverStatuses = null, IEnumerable<CarTypeOfUse> carTypesOfUse = null,
			 IEnumerable<CarOwnType> carOwnTypes = null, int[] geoGroupIds = null)
		{
			AtWorkDriver atWorkDriverAlias = null;
			var querry = uow.Session.QueryOver(()=> atWorkDriverAlias)
				.Where(x => x.Date == date);

			if (driverStatuses != null)
			{
				querry = querry.WhereRestrictionOn(awd => awd.Status).IsIn(driverStatuses.ToArray());
			}

			if(carTypesOfUse != null)
			{
				querry
					.JoinQueryOver(awd => awd.Car)
					.JoinQueryOver(c => c.CarModel)
					.WhereRestrictionOn(cm => cm.CarTypeOfUse).IsIn(carTypesOfUse.ToArray());
			}

			if(carOwnTypes != null)
			{
				Car carAlias = null;
				CarVersion carVersionAlias = null;

				querry
					.JoinAlias(() => atWorkDriverAlias.Car, () => carAlias)
					.JoinEntityAlias(() => carVersionAlias,
						() => carVersionAlias.Car.Id == carAlias.Id
							  && carVersionAlias.StartDate <= date
							  && (carVersionAlias.EndDate == null || carVersionAlias.EndDate >= date))
					.WhereRestrictionOn(() => carVersionAlias.CarOwnType).IsIn(carOwnTypes.ToArray());
			}

			if(geoGroupIds != null)
			{
				querry.WhereRestrictionOn(awd => awd.GeographicGroup.Id).IsIn(geoGroupIds);
			}

			querry = querry.Fetch(SelectMode.Fetch, x => x.Employee);
			return querry.List();
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
