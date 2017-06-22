using System;
using System.Collections.Generic;
using QSOrmProject;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Repository.Logistics
{
	public static class AtWorkRepository
	{
		public static IList<AtWorkDriver> GetDriversAtDay(IUnitOfWork uow, DateTime date)
		{
			return uow.Session.QueryOver<AtWorkDriver>()            
					  .Where(x => x.Date == date)
				      .Fetch(x => x.Employee).Eager
				      .List();
		}

		public static IList<AtWorkForwarder> GetForwardersAtDay(IUnitOfWork uow, DateTime date)
		{
			return uow.Session.QueryOver<AtWorkForwarder>()
					  .Where(x => x.Date == date)
				      .Fetch(x => x.Employee).Eager
					  .List();
		}

	}
}
