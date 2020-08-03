using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Logistic
{
	public interface IAtWorkRepository
	{
		IList<AtWorkDriver> GetDriversAtDay(IUnitOfWork uow, DateTime date, AtWorkDriver.DriverStatus? status = null);
		IList<AtWorkForwarder> GetForwardersAtDay(IUnitOfWork uow, DateTime date);
	}
}