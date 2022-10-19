using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;

namespace Vodovoz.EntityRepositories.Logistic
{
	public interface IAtWorkRepository
	{
		IList<AtWorkDriver> GetDriversAtDay(IUnitOfWork uow, DateTime date, IEnumerable<AtWorkDriver.DriverStatus> driverStatuses = null,
			IEnumerable<CarTypeOfUse> carTypesOfUse = null, IEnumerable<CarOwnType> carOwnTypes = null, int[] geoGroupIds = null);
		IList<AtWorkForwarder> GetForwardersAtDay(IUnitOfWork uow, DateTime date);
	}
}
