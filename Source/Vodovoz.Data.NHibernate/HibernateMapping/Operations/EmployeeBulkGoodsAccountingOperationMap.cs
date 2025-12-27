using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Operations
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
