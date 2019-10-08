using System;
using System.Collections.Generic;
using NHibernate;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class AtWorkRepository : IAtWorkRepository
	{
		public IList<AtWorkDriver> GetDriversAtDay(IUnitOfWork uow, DateTime date)
		{
			return uow.Session.QueryOver<AtWorkDriver>()
					  .Where(x => x.Date == date)
					  .Fetch(SelectMode.Fetch, x => x.Employee)
					  .List();
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
