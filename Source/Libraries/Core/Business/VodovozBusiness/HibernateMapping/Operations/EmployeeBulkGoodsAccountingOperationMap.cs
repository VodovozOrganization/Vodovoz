using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HibernateMapping.Operations
{
	public class EmployeeBulkGoodsAccountingOperationMap : SubclassMap<EmployeeBulkGoodsAccountingOperation>
	{
		public EmployeeBulkGoodsAccountingOperationMap()
		{
			DiscriminatorValue(nameof(OperationType.EmployeeBulkGoodsAccountingOperation));
			References(x => x.Employee).Column("employee_id");
		}
	}
}
