using System.Collections.Generic;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Operations;

namespace Vodovoz.Infrastructure.Persistance.Operations
{
	internal sealed class EmployeeNomenclatureMovementRepository : IEmployeeNomenclatureMovementRepository
	{
		public int GetDriverTerminalBalance(IUnitOfWork uow, int driverId, int terminalId)
		{
			Nomenclature nomenclatureAlias = null;
			Employee employeeAlias = null;

			var res = uow.Session.QueryOver<EmployeeNomenclatureMovementOperation>()
				.Left.JoinAlias(x => x.Nomenclature, () => nomenclatureAlias)
				.Left.JoinAlias(x => x.Employee, () => employeeAlias)
				.Where(() => employeeAlias.Id == driverId)
				.And(() => nomenclatureAlias.Id == terminalId)
				.SelectList(list => list
					.SelectSum(x => x.Amount))
				.SingleOrDefault<decimal>();
			return (int)res;
		}

		public IList<EmployeeBalanceNode> GetNomenclaturesFromDriverBalance(IUnitOfWork uow, int driverId)
		{
			Employee employeeAlias = null;
			Nomenclature nomenclatureAlias = null;
			EmployeeBalanceNode resultAlias = null;

			return uow.Session.QueryOver<EmployeeNomenclatureMovementOperation>()
					  .Left.JoinAlias(x => x.Nomenclature, () => nomenclatureAlias)
					  .Left.JoinAlias(x => x.Employee, () => employeeAlias)
					  .Where(() => employeeAlias.Id == driverId)
					  .SelectList(list => list
										  .SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
										  .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
										  .SelectSum(x => x.Amount).WithAlias(() => resultAlias.Amount))
					  .TransformUsing(Transformers.AliasToBean<EmployeeBalanceNode>())
					  .List<EmployeeBalanceNode>();
		}

		public EmployeeBalanceNode GetTerminalFromDriverBalance(IUnitOfWork uow, int driverId, int terminalId)
		{
			Employee employeeAlias = null;
			Nomenclature nomenclatureAlias = null;
			EmployeeBalanceNode resultAlias = null;

			return uow.Session.QueryOver<EmployeeNomenclatureMovementOperation>()
					  .Left.JoinAlias(x => x.Nomenclature, () => nomenclatureAlias)
					  .Left.JoinAlias(x => x.Employee, () => employeeAlias)
					  .Where(() => employeeAlias.Id == driverId)
					  .And(() => nomenclatureAlias.Id == terminalId)
					  .SelectList(list => list
										  .SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
										  .Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
										  .SelectSum(x => x.Amount).WithAlias(() => resultAlias.Amount))
					  .TransformUsing(Transformers.AliasToBean<EmployeeBalanceNode>())
					  .SingleOrDefault<EmployeeBalanceNode>();
		}
	}
}
