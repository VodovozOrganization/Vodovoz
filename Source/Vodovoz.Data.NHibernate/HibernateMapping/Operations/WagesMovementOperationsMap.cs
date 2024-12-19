using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Operations
{
	public class WagesMovementOperationsMap : ClassMap<WagesMovementOperations>
	{
		public WagesMovementOperationsMap()
		{
			Table("wages_movement_operations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.OperationTime).Column("operation_time");
			Map(x => x.Money).Column("money");
			Map(x => x.OperationType).Column("operation_type");

			References(x => x.Employee).Column("employee_id");
		}
	}
}
