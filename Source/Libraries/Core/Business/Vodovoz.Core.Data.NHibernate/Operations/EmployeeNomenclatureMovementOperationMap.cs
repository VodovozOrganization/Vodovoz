using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Operations;

namespace Vodovoz.Core.Data.NHibernate.Operations
{
	public class EmployeeNomenclatureMovementOperationMap : ClassMap<EmployeeNomenclatureMovementOperation>
	{
		public EmployeeNomenclatureMovementOperationMap()
		{
			Table("employee_nomenclature_movement_operations");
			Not.LazyLoad();

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.OperationTime)
				.Column("operation_time")
				.Not.Nullable();

			Map(x => x.Amount)
				.Column("amount")
				.Not.Nullable();

			References(x => x.Nomenclature)
				.Column("nomenclature_id")
				.Not.Nullable();

			References(x => x.Employee)
				.Column("employee_id")
				.Not.Nullable();
		}
	}
}
