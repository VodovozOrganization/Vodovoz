using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories.Operations;

namespace Vodovoz.Infrastructure.Persistance.Operations
{
	internal sealed class WagesMovementRepository : IWagesMovementRepository
	{
		public decimal GetCurrentEmployeeWageBalance(IUnitOfWork uow, int employeeId)
		{
			return uow.Session.QueryOver<WagesMovementOperations>()
				.Where(w => w.Employee.Id == employeeId)
				.Select(Projections.Sum<WagesMovementOperations>(w => w.Money))
				.SingleOrDefault<decimal>();
		}

		public decimal GetDriverWageBalanceWithoutFutureFines(IUnitOfWork uow, int employeeId)
		{
			return uow.Session.QueryOver<WagesMovementOperations>()
				.Where(w => w.Employee.Id == employeeId)
				.Where(w => w.OperationTime.Date <= DateTime.Now.Date)
				.Select(Projections.Sum<WagesMovementOperations>(w => w.Money))
				.SingleOrDefault<decimal>();
		}

		public decimal GetDriverFutureFinesBalance(IUnitOfWork uow, int employeeId)
		{
			return uow.Session.QueryOver<WagesMovementOperations>()
				.Where(w => w.Employee.Id == employeeId)
				.Where(w => w.OperationTime.Date > DateTime.Now.Date)
				.Select(Projections.Sum<WagesMovementOperations>(w => w.Money))
				.SingleOrDefault<decimal>();
		}
	}
}

