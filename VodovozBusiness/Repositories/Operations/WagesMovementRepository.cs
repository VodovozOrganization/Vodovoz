using System;
using QSOrmProject;
using Vodovoz.Domain.Operations;
using NHibernate.Criterion;

namespace Vodovoz.Repository.Operations
{
	public static class WagesMovementRepository
	{
		public static decimal GetCurrentEmployeeWageBalance (IUnitOfWork uow, int employeeId)
		{
			return uow.Session.QueryOver<WagesMovementOperations>()
				.Where(w => w.Employee.Id == employeeId)
				.Select(Projections.Sum<WagesMovementOperations>(w => w.Money))
				.SingleOrDefault<decimal>();
		}
	}
}

