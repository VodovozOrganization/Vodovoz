using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HibernateMapping.Operations
{
	public class EmployeeInstanceGoodsAccountingOperationMap : SubclassMap<EmployeeInstanceGoodsAccountingOperation>
	{
		public EmployeeInstanceGoodsAccountingOperationMap()
		{
			DiscriminatorValue(nameof(OperationType.EmployeeInstanceGoodsAccountingOperation));
			References(x => x.Employee).Column("employee_id");
		}
	}
}
