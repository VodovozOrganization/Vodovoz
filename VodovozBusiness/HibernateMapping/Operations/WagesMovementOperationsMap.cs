using Vodovoz.Domain.Operations;
using FluentNHibernate.Mapping;

namespace Vodovoz.HMap
{
	public class WagesMovementOperationsMap : ClassMap<WagesMovementOperations>
	{
		public WagesMovementOperationsMap()
		{
			Table ("wages_movement_operations");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.OperationTime)	.Column ("operation_time");
			Map (x => x.Money)			.Column("money");
			Map (x => x.OperationType)	.Column ("operation_type").CustomType<WagesTypeStringType> ();

			References (x => x.Employee).Column ("employee_id");
		}
	}
}

